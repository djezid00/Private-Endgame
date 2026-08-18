# Usklađenost rada s „Uputama za pisanje diplomskog rada" (FESB, lipanj 2017.)

**Revizija 3 — 18.8.2026.** Uspoređeno: `ANALIZA KOMPETITIVNE INTERAKCIJE… .docx`
(784 elementa, 32 slike, 11 tablica, 3 isječka koda, 16 bibliografskih jedinica) protiv
`Upute za pisanje diplomskog rada.doc`. Prethodne revizije: 13.8. (27 slika, 8 tablica),
prva (14 slika, 5 tablica). Nijedna izvorna datoteka nije mijenjana.

---

## 1. Pregled usklađenosti

| Zahtjev iz Uputa | Stanje |
|---|---|
| Naslovnica, Zadatak, Izjava, Sadržaj | ✅ |
| Numeracija stranica (dolje desno, od Uvoda) | ✅ |
| Margine 2,5 cm, Times New Roman 12 pt, prored 1,5, obostrano poravnanje | ✅ |
| Naslov poglavlja 14 pt bold, VELIKIM SLOVIMA | ✅ |
| Potpoglavlje 1. razine 12 pt bold | ⚠️ i dalje **13 pt** |
| Najviše tri razine naslova | ⚠️ četvrta razina i dalje u uporabi |
| Svako poglavlje počinje na novoj stranici | ⚠️ nije dosljedno |
| **Naslov tablice IZNAD tablice** | 🟡 novije tablice ispravno; starije i dalje ispod |
| **Numeracija tablica po poglavljima** | ✅ **riješeno** — `Tablica 5.1`…`6.6` |
| **Numeracija slika po poglavljima** | ✅ **riješeno** — `Slika 2.1`…`6.18` |
| **Numeracija kodova po poglavljima** | ✅ **riješeno** — `Kod 3.1`, `4.1`, `4.2` |
| Naslov slike ispod slike, centrirano | ✅ |
| **Kazala osvježena i točna** | ✅ **riješeno** — Kazalo slika 32, tablica 11, kodova 3 |
| **Izvor naveden uz preuzetu sliku** | ❌ **pet slika bez izvora** — v. §4.2 |
| **Prijevod stranih pojmova na preuzetim slikama** | ❌ nijedna preuzeta slika nije prevedena |
| **Numeriranje formula (2.1), (2.2)…** | ❌ nijedna jednadžba nije numerirana |
| Opis oznaka nakon formule („gdje je:") | ⚠️ neujednačeno |
| Decimalni zarez umjesto točke | 🟡 bitno bolje; provjeriti `0.5` u 3.4.6 i `99.4 %` u Uvodu |
| **Format literature prema Uputama** | ❌ IEEE stil; [1]–[4] bez autora |
| **Neodređeno / treće lice množine, pasiv** | ❌ poglavlje 2 i dalje u prvom licu množine |
| **Zaključak (do 2 str.)** | ❌ prazan |
| **Popis oznaka i kratica** | ✅ 25 stavki |
| **Sažetak, Ključne riječi, Title, Summary, Keywords** | ❌ prazni |
| Uvod do 2 stranice | ⚠️ opseg da, sadržaj i dalje tuđi (v. §3) |
| **Dokumentacija programske podrške** | 🟡 tri isječka koda u tekstu; „Ostali prilozi" i dalje prazno |
| Uravnotežen opseg poglavlja | ✅ |

---

## 2. Riješeno od prošle revizije

- **Numeracija tablica po poglavljima** — bila je posljednja nedosljednost u sustavu numeriranja;
  slike, tablice i kodovi sada koriste istu shemu `X.Y`.
- **Sva tri kazala osvježena** (`Ctrl+A` → `F9`). Kazalo kodova više ne prikazuje
  „No table of figures entries found", nego tri stvarna listinga.
- **Naslovi tablica iznad tablice** za sve novije tablice.
- **Decimalni zarezi** provedeni u Tablici 6.3, gdje su prije bile točke.
- **Tipfeleri i nedostajući razmaci** u naslovima slika poglavlja 6 ispravljeni.

---

## 3. Obavezni dijelovi koji i dalje nedostaju

| Dio | Zahtjev iz Uputa |
|---|---|
| **Zaključak** | „sažimaju se rezultati… ne bi trebao biti duži od dvije stranice" |
| **Sažetak** | „kratki pregled… u obimu do jedne stranice" |
| **Ključne riječi** | „3 do 5 riječi" |
| **Title / Summary / Keywords** | engleske inačice |
| **Ostali prilozi i dokumentacija** | Dodatak A: poveznica na repozitorij i popis priloženih datoteka |

**Uvod i dalje sadrži tekst iz tuđeg rada** — odlomci o Taxi aplikacijama, Apache Kafki, WebSocketu,
Firebaseu i „FUBAR Taxiju", plus vidljivo **„Error! Reference source not found."** umjesto izvora [3].
Nepromijenjeno kroz četiri revizije i i dalje **najveći rizik za dojam pri predaji**.

### Redoslijed završnih dijelova

Upute propisuju: **Literatura → Popis oznaka i kratica → Sažetak → Ključne riječi → Title → Summary
→ Keywords → Dodatak A…** Rad ima Kazalo slika/tablica/kodova između Literature i Popisa oznaka;
Sažetak treba doći **prije** priloga.

---

## 4. Slike, tablice i formule

### 4.1 Tablice

Numeracija je riješena. Preostaje: **naslovi starijih tablica premjestiti iznad tablice**, i naslov
„Tablica 6.1 Loss term metrika" prevesti u cijelosti na hrvatski (npr. „Vrijednosti funkcija
gubitka") — Upute traže hrvatski književni jezik i u naslovima.

### 4.2 Slike — izvor nedostaje kod pet preuzetih slika ❌

Upute: uz preuzetu sliku mora stajati izvor. Rad to ispravno radi kod `Slike 2.3` [4], `3.3` [11],
`3.5` i `4.2` [14], ali **ne i kod sljedećih**:

| Slika | Vjerojatan izvor |
|---|---|
| `Slika 2.1` Interakcija agenta s okolinom | [4] |
| `Slika 2.2` Načini modeliranja vrijednosti / politike / okoliša | [4] |
| `Slika 3.1` Pseudokod PPO | **[9] Schulman i sur.** |
| `Slika 3.2` Usporedba na MuJoCo okruženjima | **[9] Schulman i sur.** |
| `Slika 3.4` Prikaz testnih okruženja | **[11] Cohen i sur.** |

Ovo je istovremeno formalni zahtjev Uputa i pitanje atribucije — v.
`EVALUACIJA_diplomskog_rada.md` §4.

Uz to i dalje vrijedi zahtjev za **prijevodom stranih pojmova** na preuzetim slikama. Najjednostavnije
rješenje bez ponovnog crtanja jest proširiti naslov slike popisom prijevoda, npr.
*„Slika 3.2. Usporedba algoritama na okruženjima MuJoCo (engl. `Hopper-v1` — skakač, `Walker2d-v1`
— dvonožni hodač…). Preuzeto iz: [9]"*.

### 4.3 Formule — nijedna nije numerirana ❌

Nepromijenjeno. Uz to, u 5.5 i dalje postoje **prazna mjesta gdje su jednadžbe ispale iz teksta**
(„prema formuli .", „dok pri  pada", „(; ; )"). **Ta se mjesta nalaze isključivo u starijoj,
dvostrukoj verziji potpoglavlja** — brisanjem te verzije problem nestaje sam od sebe
(v. `DUPLIKATI_u_poglavljima_5_i_6.md` §1).

---

## 5. Jezik i stil

**Poglavlje 2 i dalje u prvom licu množine** („Recimo da…", „Definirajmo…", „Zamislimo…",
„Označimo…", „Možemo reći…") — 8 mjesta. Poglavlja 4–6 pisana su ispravno.

**Dvojezični naslovi** u 6.4.1 i 6.4.2 („Sparse arm", „Shaped arm"). Prijedlog:
*„Rijetka nagrada — emergentni progon"* i *„Oblikovana nagrada — oblikovno farmiranje i inverzija
rezultata"*, uz engleski naziv u zagradi pri prvom spominjanju.

**Doslovan navod iz ML-Agents dokumentacije** (5.4) ostao je na engleskom; Upute traže hrvatski
prijevod uz izvorni navod.

**Nedosljedna kratica** — koristi se i `eng.` i `engl.`; Upute navode `engl.`

---

## 6. Literatura

Popis je narastao na **16 jedinica**, ali format je nepromijenjen: **IEEE stil** (inicijali ispred
prezimena) za većinu, dok [1]–[4] **nemaju autora**, što Upute za internetske izvore izričito traže.

| Jedinica | Problem | Prijedlog |
|---|---|---|
| [1] NVIDIA povijest | nema autora | navesti instituciju kao autora |
| [2] Lee Sedol / AlphaGo | nema autora | `Gomagic: "Lee Sedol and AlphaGo…", s Interneta, …` |
| [3] Dota 2 | nema autora, nema „s Interneta" | `OpenAI i dr.: "Dota 2 with Large Scale Deep RL", …` |
| [4] Lilian Weng | nema autora | `Weng, L.: "A (Long) Peek into Reinforcement Learning", …` |
| ostale | IEEE format | preurediti u `Prezime, I.: "Naslov", Izdavač, mjesto, godina.` |

**Nove jedinice koje treba dodati** (v. `EVALUACIJA_diplomskog_rada.md` §4): Elo (1978.) uz ELO
formule, Skinner (1938.) uz podrijetlo oblikovanja nagrade, Isaacs (1965.) uz tvrdnje iz teorije
progona i bijega.

---

## 7. Redoslijed izmjena

**A — obavezno prije predaje:**

1. Prepisati Uvod; popraviti „Error! Reference source not found.".
2. Napisati Zaključak, Sažetak, Ključne riječi, Title, Summary, Keywords.
3. Dodati Dodatak A i premjestiti Sažetak ispred priloga.
4. Dodati izvore uz pet preuzetih slika i tri nedostajuće reference (§4.2, §6).

**B — mehaničke izmjene:**

5. Provesti zahvate iz `DUPLIKATI_u_poglavljima_5_i_6.md` (usput nestaju prazne jednadžbe i dvije
   pokvarene unakrsne upute).
6. Numerirati formule po poglavljima.
7. Premjestiti naslove starijih tablica iznad tablica.
8. `Naslov2` na 12 pt; *Page break before* u stilu `Naslov1`.
9. Provjeriti preostale decimalne točke (`0.5`, `99.4 %`).

**C — sadržajne dorade:**

10. Poglavlje 2 u neodređeno/treće lice množine (8 mjesta).
11. Prevesti strane pojmove na preuzetim slikama i navod iz dokumentacije u 5.4.
12. Prevesti naslove 6.4.1 i 6.4.2.
13. Ujednačiti `eng.` / `engl.`
