# Vodič za dovršetak diplomskog rada — revizija 3 (18.8.2026.)

> **Što je ovo.** Objedinjena radna lista za ručni dovršetak `.docx`-a, provjerena izravnim čitanjem
> trenutnog dokumenta (784 elementa, 32 slike, 11 tablica, 3 isječka koda). Claude ne dira `.docx`.
>
> **Prateći dokumenti:**
> - `DUPLIKATI_u_poglavljima_5_i_6.md` — **novo**, detaljan popis ponavljanja i točnih zahvata
> - `EVALUACIJA_diplomskog_rada.md` — sadržajna ocjena + provjera atribucije
> - `USKLADENOST_s_uputama_FESB.md` — formalna usklađenost s Uputama
>
> **Status Faze B:** odgođena, nije otkazana. Rad to sada i eksplicitno navodi u 5.6, što je ispravno.

---

## 1. Riješeno od prošle revizije ✅

| Bilo | Sada |
|---|---|
| Eksperiment 2 (POCA vs PPO) bez rezultata | ✅ **Novo potpoglavlje 6.2 + Tablica 6.2** — rasprava više ne citira nepokazane podatke |
| Sva tri kazala zastarjela | ✅ Osvježena (`F9`): 32 slike, 11 tablica, 3 koda |
| γ-tablica bez naslova, s markdown zvjezdicama | ✅ „Tablica 6.6", zvjezdice uklonjene |
| Tablica 400k bez retka sa stopom hvatanja | ✅ Redak `Environment/Catch` dodan |
| Decimalne točke u tablici 400k | ✅ Zarezi |
| Tipfeleri i nedostajući razmaci u naslovima slika | ✅ Ispravljeno |
| `Kod 31`, `41`, `42` | ✅ `Kod 3.1`, `4.1`, `4.2` |
| Tablice numerirane linearno | ✅ Po poglavljima (`5.1`…`6.6`) |
| Faza A/B bez obrazloženja | ✅ Novo obrazloženje u 5.6 + izjava da Faza B nije provedena |
| Nema dokaza za MA-POCA | ✅ `Slika 6.1` (Baseline Loss, POCA vs PPO) |

---

## 2. Prioritet 1 — ponavljanja u poglavljima 5 i 6 🔴

**Ovo je sada najveći problem u radu i najbrže se rješava jer je uglavnom brisanje.**
Potpuni popis zahvata: **`DUPLIKATI_u_poglavljima_5_i_6.md`**. Sažetak:

| # | Što | Zahvat |
|---|---|---|
| D1 | **5.5 sadrži dvije potpune verzije istog eksperimenta**; starija ima prazne jednadžbe | Obriši stariju verziju — **usput nestaju sve tri prazne jednadžbe** |
| D2 | 5.5 sadrži rezultate (dizajn ≠ rezultati) | Premjesti tri odlomka u 6.5.3; ovime **postaje točna** i uputa iz 6.4.2 koja sada vodi na krivo mjesto |
| D3 | 5.6 sadrži rezultate Faze A koje poglavlje 6 ponavlja | Obriši tri odlomka, dodaj veznu rečenicu |
| D4 | „Zamka ima dva uzroka" napisano dvaput (6.2 i 6.5.2) | U 6.5.2 izbaci ponovljenu brojku, zamijeni uputom na Tablicu 6.2 |
| D5 | Inverzija 400k → 5M ispričana triput | Zadrži u 6.5.1, obriši iz 6.4.2, popravi nabrajanje na dvije stavke |
| D6 | Dvije pokvarene upute: „potpoglavlju **0**", „poglavlje **0**" | Obje trebaju pokazivati na 6.4.2 |

---

## 3. Prioritet 2 — Uvod 🔴

| # | Problem | Zahvat |
|---|---|---|
| U1 | Odlomci o **Taxi aplikacijama, Apache Kafki, WebSocketu, Firebaseu, „FUBAR Taxiju"** — tekst iz tuđeg rada, nepromijenjen kroz četiri revizije | Izbaci. Nije vezano ni uz što odgođeno |
| U2 | **„Error! Reference source not found."** umjesto izvora [3] | Ponovno umetni referencu |
| U3 | „Rad je podijeljen na **sedam** poglavlja" | 🟡 Odgodi dok struktura ne bude konačna (§7) |

---

## 4. Prioritet 3 — atribucija (~1 h) 🔴

Detaljno u `EVALUACIJA_diplomskog_rada.md` §4. Sažetak:

| # | Mjesto | Zahvat |
|---|---|---|
| C1 | `Slika 2.1`, `2.2`, `3.1`, `3.2`, `3.4` — preuzete, **bez navedenog izvora** | Dodaj „Preuzeto iz: [4] / [9] / [11]" u naslov slike, kao kod `Slike 3.3` |
| C2 | **ELO formule** (3.5.3) bez izvora | Citiraj *Elo, A. E.: „The Rating of Chessplayers, Past and Present", Arco, New York, 1978.* |
| C3 | **Podrijetlo shapinga** iz bihevioralne psihologije, „successive approximations" (4.6.1) | Citiraj Skinnera (1938.) ili [12] |
| C4 | **Tvrdnje iz teorije progona i bijega** (3.4.6 i 6.5.4) — nose zaključak RQ-C | Citiraj *Isaacs, R.: „Differential Games", Wiley, New York, 1965.* |
| C5 | „Stalni član" predstavljen isključivo kao vlastiti nalaz (3.4.6) | Dodaj rečenicu koja priznaje raniji rad (Wiewiora 2003., Grześ 2017.); doprinos ovog rada je **kvantifikacija** i veza s farming-zamkom |

---

## 5. Prioritet 4 — brojčane i formalne ispravke

| # | Mjesto | Problem |
|---|---|---|
| A16 | 4.6 | „od 4 na 16 → sa ~277 na ~553" **spaja dva mjerenja**: 277 je smoke-test s 4 arene; bake-off je 12 → 16 arena (495 → 553, +12 %) |
| F1 | cijeli rad | **Nijedna jednadžba nije numerirana** |
| F2 | starije tablice | Naslov ispod tablice; treba iznad |
| F3 | 6.4.1 / 6.4.2 | Dvojezični naslovi („Sparse arm", „Shaped arm") |
| F4 | poglavlje 2 | Prvo lice množine, 8 mjesta |
| F5 | literatura | IEEE format; [1]–[4] bez autora |
| F6 | 5.4 | Doslovan navod iz dokumentacije ostao na engleskom |

---

## 6. Slike koje još treba izraditi

| Mjesto | Grafika |
|---|---|
| **5.4** | **Dijagram toka nagrade MA-POCA vs PPO** — najveća vrijednost za uloženi trud; nosi objašnjenje zamke |
| 4.2 | Tortni dijagram profila vremena (env_step 78 %, inference 14 %, gradijenti 6 %) |
| 4.4 | Shematski tlocrt arene 20×20 s raycast zrakama |
| 3.4.6 | Graf Φ(s) + stalnog člana koji raste s udaljenošću |

---

## 7. Odgođeno — piši na kraju

- **Zaključak** (do 2 str.) — prazan
- **Sažetak** + **Ključne riječi** — prazni
- **Title / Summary / Keywords** — prazni
- **Ostali prilozi i dokumentacija** — Dodatak A (repozitorij `Private-Endgame`, popis priloga,
  `.onnx` modeli u `Assets/Models/5M/`)
- **Uvod, broj poglavlja** — uskladiti s konačnom strukturom

Sav materijal postoji u `Theory.md` §§11–15 i u poglavlju 6 — riječ je o sažimanju, ne o novom
istraživanju.

---

## 8. Redoslijed

1. **Ponavljanja** (§2) — najveći učinak, uglavnom brisanje, usput rješava prazne jednadžbe i
   pokvarene upute.
2. **Uvod** (§3).
3. **Atribucija** (§4).
4. Brojčane i formalne ispravke (§5).
5. Preostale slike (§6).
6. Zaključak, Sažetak, Dodatak A (§7).
