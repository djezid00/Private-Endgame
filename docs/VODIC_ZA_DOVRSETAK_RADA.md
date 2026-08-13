# Vodič za dovršetak diplomskog rada — revizija 2 (13.8.2026.)

> **Što je ovo.** Radna lista za ručni dovršetak `.docx`-a. Revizija 1 (9.8.2026.) spajala je
> `EVALUACIJA_diplomskog_rada.md` i `USKLADENOST_s_uputama_FESB.md`. Ova revizija je **ponovno
> provjerena izravnim čitanjem trenutnog `.docx`-a (13.8.2026., 725 odlomaka, 8 tablica, 27 slika,
> 3 isječka koda, 33 medijske datoteke)** — rad je od prošle revizije **narastao otprilike
> dvostruko** (XML 478 kB → 968 kB). Claude ne dira `.docx`; sve se radi ručno u Wordu.
>
> **Status Faze B (ispravak, potvrđeno 9.8.2026.):** Faza B je **odgođena, nije otkazana**. Redoslijed
> je: dovrši teoriju i pisanje za Fazu A i sve prije nje → *zatim* pokreni Fazu B. Prva radnja pri
> povratku na eksperimente ostaje rebuild + `TagMApoca_obs_smoke` gate. `EVALUACIJA` N1/N2 to i dalje
> bilježi pogrešno („Faza B otpada") — ispravljeno u ovoj reviziji.
>
> **Odgođeno do kraja (ne piši još):** Zaključak, Sažetak/Ključne riječi, Title/Summary/Keywords i
> ispravak broja poglavlja u Uvodu — čekaju konačan oblik rada (§7).

---

## 1. Što je riješeno od prošle revizije ✅

Velik napredak. Provjereno u tekstu, ne po sjećanju:

| Bilo | Sada |
|---|---|
| **Popis oznaka i kratica prazan** | ✅ **Popunjen, 25 stavki**, abecedno (odl. 688–712) |
| Numeracija slika `Slika 21`, `Slika 41` (bez točke) | ✅ **`Slika 2.1` … `Slika 6.17`** — po poglavljima, s točkom |
| Naslovi poglavlja miješana veličina slova | ✅ **Svih 7 naslova VELIKIM SLOVIMA** |
| „Pregled korištenih **rml** Algoritama" | ✅ **„PREGLED KORIŠTENIH RML ALGORITAMA"** |
| Parametri samoigre `⟨save_steps⟩` u zagradama | ✅ **Popunjeni**: `save_steps` = `swap_steps` = 50 000, `team_change` = 100 000; validacija 400k = 25 000/25 000/50 000 (odl. 495–501) |
| Bilješka „EXPLAIN: Konkretni parametri samoigre…" | ✅ Uklonjena, zamijenjena stvarnim tekstom |
| „stopa hvatanja … 1,00 **pri γ=0,99**" (netočno) | ✅ **Ispravljeno na γ=0,95** (odl. 566) |
| Nedosljednost 78 % / ~80 % | ✅ **Ujednačeno na 78 %** (odl. 329 i 508) |
| U radu nema nijednog isječka koda | ✅ **Tri listinga**: `Kod 31` (oblikovanje nagrade), `Kod 41` (funkcija oblikovanja), `Kod 42` (konfiguracijska datoteka) + `OnAgentTagged` u §4.5 |
| Poglavlje 6 bez slika | ✅ **17 slika (6.1–6.17)**: 5 × validacija 400k, 6 × sparse 5M, 6 × shaped 5M |
| Nedostaju tablice rezultata | ✅ **Tablica 5** (400k validacija), **Tablica 6** (sparse 5M), **Tablica 7** (shaped 5M) + **nenumerirana γ-tablica** (9 pokreta) |
| Naslovi tablica ispod tablice | 🟡 **Djelomično**: Tablice 5, 6, 7 imaju naslov **iznad** (ispravno); Tablice 1–4 i dalje **ispod** |
| §4.6 tanak, bez scene | ✅ Znatno proširen + novo potpoglavlje **„Scena"** sa `Slika 4.2` (16 arena) — riješen jedan TODO |

**Provjera brojeva:** sve vrijednosti u Tablicama 5/6/7 i u γ-tablici **točno odgovaraju** `Theory.md`
(ELO 1890,7 / 685,5 / 661,1; GroupR +1,45 / −0,87 / −0,94; shaped −0,98/−1,00/−0,96 uz Mean Reward
5,38/3,93/4,29; γ-tablica svih 9 redaka). Nema izmišljenih brojeva.

---

## 2. Novo uočeno — nastalo pri unosu (popravi prvo, brzo je) 🔴

Ovo su **nove greške koje prije nisu postojale**, nastale kopiranjem iz markdowna:

| # | Mjesto | Problem | Popravak |
|---|---|---|---|
| **N1** | γ-tablica (§6.4.3, iza odl. 648) | **Ostali `**` znakovi iz markdowna**: ćelija piše `**1,00**` umjesto `1,00` | Obriši zvjezdice; podebljaj Wordovim boldom ako želiš isticanje |
| **N2** | ista γ-tablica | **Nema naslov ni broj** — jedina tablica u radu bez `Opisslike` naslova ⇒ neće ući u Kazalo tablica | Dodaj naslov **iznad**: „Tablica 6.4. Rezultati Faze A — devet konfiguracija γ pretraživanja" (broj prilagodi shemi iz N7) |
| **N3** | Tablica 5 (400k) | **Nedostaje redak `Environment/Catch`** — a to je glavna ishodna metrika, tekst je spominje („~2,5–3× veću stopu hvatanja") i `Slika 6.1` je prikazuje | Dodaj redak: `Environment/Catch (stopa hvatanja)` \| `~0,08` \| `~0,21` \| `oblikovanje ≈ 2,5–3× veća stopa hvatanja` |
| **N4** | Tablica 5 (400k) | **Decimalne točke** (`1212.6`, `+21.9`, `−0.91`) dok Tablice 6/7 koriste **zareze** (`1890,7`) | Zamijeni sve točke zarezima — Upute traže decimalni zarez |
| **N5** | 15 naslova slika/tablica | **Nedostaje razmak iza broja**: `Slika 6.1Stopa…`, `Slika 3.3Uzoračka…`, a najgore **`Tablica 5400k validacija`** (čita se kao „Tablica 5400k") | Dodaj razmak iza broja u svih 15 (`Slika 3.3`, `3.4`, `3.5`, `6.1`–`6.5`, `6.7`–`6.9`, `6.12`–`6.15`, `Tablica 5`, `Tablica 7`) |
| **N6** | naslovi slika u pogl. 6 | **Tipfeleri**: `Envirnoment` (4×), `Lenght` (2×), `Culmutive`, `Cumultive`, `Rezultait`, `objeruke` (2×), `grafovasnimljenih` | Ispravi: Environment, Length, Cumulative, Rezultati, „obje ruke", „grafova snimljenih" |
| **N7** | sve tablice | Numeracija i dalje **linearna** (`Tablica 1`…`7`) iako su slike prešle na **po poglavljima** (`Slika 6.1`) — nedosljedno unutar istog rada | Prebroji tablice po poglavljima: `Tablica 4.1` (nagrade), `5.1` (mjerne veličine), `5.2` (dizajn ruku), `6.1` (loss), `6.2` (400k), `6.3` (sparse 5M), `6.4` (shaped 5M), `6.5` (γ Faza A) |
| **N8** | naslovi kodova | `Kod 31`, `Kod 41`, `Kod 42` — bez točke, ne prate novu shemu slika | → `Kod 3.1`, `Kod 4.1`, `Kod 4.2` |
| **N9** | odl. 36–37, 598, 616 | Prazni `Opisslike` odlomci (naslovi bez sadržaja) | Obriši — inače stvaraju prazne retke u Kazalu |

---

## 3. Staro, i dalje otvoreno 🔴

| # | Mjesto | Problem | Status |
|---|---|---|---|
| **A1** | §5.6 „Faza A", odl. 563–568 | Rezultati γ-sweepa i dalje **opisani dvaput** — ovdje (prozom, s brojkama) i u §6.4.3 (gdje je i tablica) | 🔴 nepromijenjeno. Poglavlje 5 = dizajn i predikcije; **premjesti odl. 566–568 u §6.4.3** |
| **A2** | odl. 570 (uvod u pogl. 6) | Opisuje Fazu A i Fazu B kao da su **obje provedene** („istraživanje je podijeljeno u dvije faze… Faza B rješava taj problem") | 🔴 Preformuliraj u: Faza A je provedena, Faza B je **planirani nastavak koji nije proveden u okviru ovog rada** |
| **A3/A4** | odl. 568, 652 | „…motivira Fazu B … kao sljedeći korak istraživanja"; „Faza B … **ispitat će** robustnost" | 🟢 **Ovo je sada ispravno** (Faza B je odgođena, ne otkazana) — samo osiguraj da nigdje ne zvuči kao da su rezultati Faze B već dobiveni |
| **A10** | odl. 294 (§3.4.6) | Visjeća referenca **„§12 i §14"** — ne postoje takva poglavlja | 🔴 Zamijeni s „§6.4.3" (γ-sonde i stalni član) |
| **A11** | odl. 637 (§6.3.2) | **„[pogl. X.Y]"** — nepopunjen placeholder | 🔴 Zamijeni s „§6.4.3" |
| **A14** | odl. 561–562 (§5.5) | **Prazne jednadžbe**: „prema formuli **.**", „dok pri **[prazno]** pada", „u sve tri testirane konfiguracije **(; ; )**" | 🔴 Popuni: horizont ≈ 1/(1−γ); drugi γ je **0,8** (5 odluka / 25 fiz. koraka); tri konfiguracije = **γ = 0,8; 0,9; 0,99** |
| **A16** | odl. 503 (§4.6) | „Povećanje broja arena **od 4 na 16** → sa ~277 na ~553 (**+100 %**)" — **spaja dva odvojena mjerenja** | 🔴 Po `Theory.md` §10: 277 je iz **smoke-testa s 4 arene**; bake-off je **12 arena = 495 → 16 arena = 553 (+12 %)**. Prepiši u dvije rečenice |
| **A18** | odl. 115–120 (Uvod) | **Šest odlomaka o Taxi aplikacijama, Apache Kafki, WebSocketu, Firebaseu i FUBAR Taxiju** — tekst iz tuđeg rada | 🔴 **NAJVEĆI RIZIK PRI PREDAJI.** Ovo nije vezano uz Fazu B — može se i treba izbaciti odmah (zamjenski tekst tek kad se zna konačan broj poglavlja, §7) |
| **A19** | odl. 113 (Uvod) | **„Error! Reference source not found."** — pokvarena Word referenca na izvor [3] (Dota 2) | 🔴 Ponovno umetni referencu `[3]` |
| **A20** | §5.4 / poglavlje 6 | **Eksperiment 2 (MA-POCA vs PPO, matrica 2×2) i dalje ima dizajn, ali NIGDJE rezultate** — a §6.4.2 se poziva na brojku „0,12 naspram PPO-ovih 0,98" koja se nigdje ne prikazuje | 🔴 Dodaj potpoglavlje s tablicom 2×2 i sondom isporuke (podaci u §4 ovog vodiča) — inače rasprava citira nepokazane podatke |

---

## 4. Tablice koje još nedostaju (podaci gotovi, prijenos ~30 min)

Sve ostalo iz prošle revizije je uneseno. Ostaju dvije, obje vezane uz **A20**:

### T-ppo — Matrica 2×2 (POCA/PPO × sparse/shaped, 5M)
*Izvor: `Theory.md` §13. Ide u poglavlje 6 (rezultati), ne u §5.4 (dizajn).*

| Konfiguracija | Stopa hvatanja | Napomena |
|---|---|---|
| POCA + rijetka | ~1,00 | referentna točka |
| PPO + rijetka | ~0,90 | PPO neznatno slabiji od MA-POCA |
| PPO + oblikovana | ~0,98 | PPO **izbjegava** zamku |
| POCA + oblikovana | ~0,01 | **jedina ćelija koja upada u farming-zamku** |

### T-probe — Sonda kanalske isporuke nagrade
*Izvor: `Theory.md` §13. Ovo je brojka koju §6.4.2 već citira.*

| Način isporuke terminalne nagrade | Stopa hvatanja |
|---|---|
| samo grupni kanal (izvorni MA-POCA) | ~0,01 |
| grupni + individualni (`individual_terminal_reward`) | ~0,12 (djelomičan oporavak, još raste) |
| samo individualni (PPO-stil) | ~0,98 |

Zaključak koji ove dvije tablice potkrepljuju (već napisan u odl. 644–645): zamka ima **dva uzroka** —
kanal isporuke nagrade (nužan uvjet) i algoritamska osjetljivost MA-POCA (nije sam po sebi dovoljan).

---

## 5. Slike — stanje

**Uneseno (27 slika):** `2.1`–`2.3`, `3.1`–`3.5`, `4.1`, `4.2`, `6.1`–`6.17`. Poglavlje 6 je time
vizualno kompletno za validaciju 400k i za obje ruke 5M treninga.

**Preostali `TODO` markeri u tekstu (4):**

| Odl. | Što traži | Status |
|---|---|---|
| 291 | Graf potencijalne funkcije Φ(s) + stalnog člana (1−γ)·coef·(d/maxDist) | 🔨 treba izraditi (trivijalno u matplotlibu) |
| 330 | Tortni dijagram wall-clock profila (env_step 78 %, inference 14 %, gradijenti 6 %) | 🔨 treba izraditi, brojevi poznati |
| 338 | Shematski tlocrt arene 20×20 s raycast zrakama | 🔨 treba izraditi |
| 562 | Dijagram toka nagrade MA-POCA vs PPO | 🔨 treba izraditi — **najveća vrijednost za uloženi trud**, nosi cijelo objašnjenje zamke (i veže se uz A20) |
| 579 | TensorBoard screenshot `BaselineLoss` iz dim-testa | 🔨 screenshot iz `TagTest_poca_01`, ako TB podaci postoje |

**Nije obavezno, ali podiže dojam:** shema arhitekture MA-POCA kritičara (§3.4.3), 2×3 mreža sličica
iz Editor-inferencije koja vizualno suprotstavlja sparse Chasera (presijeca put) i shaped Chasera
(farmira izdaleka) u §6.3.2.

---

## 6. Prije predaje — mehanički koraci

1. **`Ctrl+A` → `F9`** (osvježi sva polja). **Ovo je sada obavezno**: Kazalo slika još uvijek prikazuje
   staru numeraciju (`Slika 21`…`Slika 41`, 9 stavki) umjesto 27 novih, Kazalo tablica prikazuje 6 od 8,
   a Kazalo kodova i dalje piše „No table of figures entries found" iako u radu **postoje tri isječka**.
2. Provjeri da svako poglavlje počinje na novoj stranici (uključi *Page break before* u stilu `Naslov1`).
3. Provjeri da su svi naslovi tablica **iznad** tablica (Tablice 1–4 još nisu).
4. Numeriraj formule po poglavljima `(2.1)`, `(3.1)`… — **nijedna od ~30 jednadžbi još nema broj**.
5. Ispravi preostale decimalne točke u prozi (`0.5` u §3.4.6, `99.4%` u Uvodu) u zareze.

---

## 7. Odgođeno — piši tek na kraju

- **Zaključak** (do 2 str.) — prazan (odl. 656). Sažima RQ-A/B/C ishode + status Faze B.
- **Sažetak** (do 1 str.) + **Ključne riječi** (3–5) — prazni.
- **Title / Summary / Keywords** (engleski) — prazni.
- **Ostali prilozi i dokumentacija** — prazno; treba Dodatak A (poveznica na repozitorij
  `Private-Endgame`, popis priloženih datoteka: konfiguracije, `.onnx` modeli u `Assets/Models/5M/`).
- **Uvod, broj poglavlja** — „Rad je podijeljen na sedam poglavlja" (odl. 115); stvarno stanje je
  6 + Zaključak. Ispravi zajedno s A18, kad struktura bude konačna.

Sav materijal za ove dijelove već postoji u `Theory.md` §§11–15 i u poglavlju 6 samog rada — riječ je
o sažimanju, ne o novom istraživanju.
