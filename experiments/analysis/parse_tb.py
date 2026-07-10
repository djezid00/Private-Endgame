"""Dependency-free tfevents scalar extractor (TFRecord + minimal protobuf).

Usage: python parse_tb.py <run_dir> [<run_dir> ...]
Prints, per run and behavior, the mean of the last 5 points of key scalar tags.
"""
import os, struct, sys, glob

TAGS = [
    "Environment/Catch",
    "Environment/TimeToCatch",
    "Environment/Cumulative Reward",
    "Environment/Group Cumulative Reward",
    "Environment/Episode Length",
    "Self-play/ELO",
]


def read_records(path):
    with open(path, "rb") as f:
        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                return
            (length,) = struct.unpack("<Q", hdr)
            f.read(4)  # len crc
            payload = f.read(length)
            f.read(4)  # data crc
            if len(payload) < length:
                return
            yield payload


def parse_varint(buf, i):
    result = shift = 0
    while True:
        b = buf[i]; i += 1
        result |= (b & 0x7F) << shift
        if not b & 0x80:
            return result, i
        shift += 7


def parse_fields(buf):
    """Yield (field_no, wire_type, value_bytes_or_int) for one protobuf message."""
    i = 0
    while i < len(buf):
        key, i = parse_varint(buf, i)
        field, wire = key >> 3, key & 7
        if wire == 0:
            val, i = parse_varint(buf, i)
        elif wire == 1:
            val = buf[i:i + 8]; i += 8
        elif wire == 2:
            ln, i = parse_varint(buf, i)
            val = buf[i:i + ln]; i += ln
        elif wire == 5:
            val = buf[i:i + 4]; i += 4
        else:
            return
        yield field, wire, val


def extract_scalars(events_file):
    """-> {tag: [(step, value), ...]}"""
    out = {}
    for rec in read_records(events_file):
        step = 0
        summary = None
        for field, wire, val in parse_fields(rec):
            if field == 2 and wire == 0:
                step = val
            elif field == 5 and wire == 2:
                summary = val
        if summary is None:
            continue
        for field, wire, val in parse_fields(summary):
            if field == 1 and wire == 2:  # Summary.Value
                tag, simple = None, None
                for f2, w2, v2 in parse_fields(val):
                    if f2 == 1 and w2 == 2:
                        tag = v2.decode("utf-8", "replace")
                    elif f2 == 2 and w2 == 5:
                        (simple,) = struct.unpack("<f", v2)
                if tag is not None and simple is not None:
                    out.setdefault(tag, []).append((step, simple))
    return out


def main():
    for run_dir in sys.argv[1:]:
        run = os.path.basename(run_dir.rstrip("/\\"))
        for beh in ("Chaser", "Runner"):
            files = sorted(glob.glob(os.path.join(run_dir, beh, "events.out.tfevents.*")))
            merged = {}
            for fpath in files:
                for tag, pts in extract_scalars(fpath).items():
                    merged.setdefault(tag, []).extend(pts)
            print(f"== {run} / {beh}")
            for tag in TAGS:
                pts = sorted(merged.get(tag, []))
                if not pts:
                    print(f"   {tag:42s} (absent)")
                    continue
                tail = [v for _, v in pts[-5:]]
                mean = sum(tail) / len(tail)
                print(f"   {tag:42s} last5={mean:10.4f}  n={len(pts)}  last_step={pts[-1][0]}")


if __name__ == "__main__":
    main()
