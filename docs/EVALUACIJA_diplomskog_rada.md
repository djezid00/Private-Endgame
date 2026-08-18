# Evaluacija diplomskog rada — revizija 4 (18.8.2026.)

Analizirano: glavni `.docx` (**784 elementa, 32 slike, 11 tablica, 3 isječka koda, 16 bibliografskih
jedinica**) i `Theory.md`. Prethodne evaluacije: 13.8. (725 odlomaka, 27 slika, 8 tablica),
6.8. (392 odlomka, 14 slika, 5 tablica), 5.8. (341 odlomak, 13 slika, 2 tablice).

> **Napomena o opsegu.** Faza B je **odgođena, nije otkazana** — pokreće se nakon što su teorija i
> pisanje za Fazu A dovršeni. Rad je sada i eksplicitno tako formulira (5.6: „Faza B nije provedena
> u okviru ovog rada"), što je ispravno.

---

## 1. Što je napravljeno od prošle evaluacije

| Dodano | Ocjena |
|---|---|
| **Potpoglavlje 6.2 „Rezultat POCA VS PPO"** s Tablicom 6.2 (matrica 2×2) | ✅ **Zatvorena najveća sadržajna rupa.** Rasprava više ne citira nepokazane podatke |
| **Sva tri kazala osvježena** (`F9`) — Kazalo slika 32 stavke, Kazalo tablica 11, Kazalo kodova 3 | ✅ |
| **`Kod 3.1`, `4.1`, `4.2`** — numeracija kodova usklađena sa shemom slika | ✅ |
| **Tablica 6.3 dopunjena retkom `Environment/Catch`** (~0,08 / ~0,21) | ✅ Riješena zamjerka N3 |
| **Markdown zvjezdice uklonjene iz γ-tablice**, tablica dobila naslov „Tablica 6.6" | ✅ Riješene zamjerke N1 i N2 |
| **Tipfeleri u naslovima slika ispravljeni** (`Environment`, `Length`, `Cumulative`) | ✅ Riješena zamjerka N6 |
| **Razmaci iza brojeva u naslovima** | ✅ Riješena zamjerka N5 |
| **Decimalni zarezi u Tablici 6.3** | ✅ Riješena zamjerka N4 |
| **Novo 5.6: obrazloženje podjele na Fazu A i B** + eksplicitna izjava da Faza B nije provedena | ✅ Riješene zamjerke A2/A4 |
| **Nova potpoglavlja 4.6.1 i 4.6.2** (rijetka/oblikovana nagrada, konfiguracija) + `Slika 4.1`, `4.3` | ✅ |
| **Slika 6.1** — dokaz MA-POCA preko `Baseline Loss`, uz PPO usporedbu | ✅ |
| Nova potpoglavlja 6.4.2 prepisana (mehanizam farmiranja) | ✅ Čitljivije, s uputama na 3.4.6 |

**Pokrivenost `Theory.md` je sada praktički potpuna** za sve što je provedeno.

---

## 2. Iskreno mišljenje

Rad je **sadržajno gotov do razine Faze A**. Poglavlje 6 sada ima sve četiri komponente — dokaz da
je korišten MA-POCA, usporedbu s PPO, validaciju i glavni rezultat — svaku s tablicom i slikom.
Argumentacija u 6.5 i dalje je najjači dio.

**Ono što sada najviše smeta više nije nedostatak sadržaja, nego njegov višak na krivim mjestima.**
Poglavlje 5.5 sadrži **dvije potpune verzije istog eksperimenta** (starija je oštećena — u njoj su
prazne jednadžbe), a poglavlja 5.5 i 5.6 sadrže rezultate koje poglavlje 6 ponavlja. Detaljna
analiza i točan popis zahvata nalaze se u **`DUPLIKATI_u_poglavljima_5_i_6.md`**; to je sada
prioritet broj jedan jer zahvat je gotovo isključivo brisanje.

**Drugo, i novo u ovoj reviziji:** rad na nekoliko mjesta iznosi tvrdnje preuzete iz literature bez
navođenja izvora, i reproducira slike iz tuđih radova bez atribucije. Popis je u §4. To nije stilska
zamjerka — to je pitanje akademske čestitosti i najlakše se rješava dodavanjem referenci koje ionako
postoje.

**Treće:** Uvod je i dalje tuđi (Taxi/Kafka/WebSocket/Firebase, uz „Error! Reference source not
found."). Nepromijenjeno kroz četiri evaluacije. Rad sada ima ozbiljno poglavlje 6, a mentor prvo
čita uvod.

---

## 3. Preostalo iz prošlih evaluacija

| # | Stavka | Status |
|---|---|---|
| 1 | **Uvod, odlomci o Taxi aplikacijama, Kafki, WebSocketu, Firebaseu** | 🔴 nepromijenjeno |
| 2 | **„Error! Reference source not found."** u Uvodu (izvor [3]) | 🔴 |
| 3 | „Rad je podijeljen na **sedam** poglavlja" — stvarno 6 + Zaključak | 🟡 odgođeno do konačne strukture |
| 4 | **Zaključak** prazan | 🟡 odgođeno |
| 5 | **Sažetak, Ključne riječi, Title, Summary, Keywords** prazni | 🟡 odgođeno |
| 6 | **„Ostali prilozi i dokumentacija"** prazno — nema Dodatka A | 🔴 |
| 7 | „povećanje broja arena od 4 na 16 → sa ~277 na ~553" — spaja smoke-test (4 arene) s bake-offom (12 → 16, +12 %) | 🔴 |
| 8 | Naslovi tablica **ispod** tablice za starije tablice | 🟡 novije su ispravne |
| 9 | Tablice numerirane po poglavljima | ✅ **riješeno** (6.1–6.6) |
| 10 | Naslovi 6.4.1 / 6.4.2 dvojezični („Sparse arm", „Shaped arm") | 🔴 |
| 11 | Poglavlje 2 u prvom licu množine (8 mjesta) | 🔴 |
| 12 | Formule nenumerirane | 🔴 |
| 13 | Literatura u IEEE formatu; [1]–[4] bez autora | 🔴 |
| 14 | Dvije pokvarene unakrsne upute („potpoglavlju **0**", „poglavlje **0**") | 🔴 v. `DUPLIKATI…` §4 |

---

## 4. Atribucija i autorska prava — provjera

Provjereno je iznosi li rad tvrdnje ili slike koje potječu iz tuđih izvora bez navođenja. Nađeno je
**pet mjesta**, poredanih po ozbiljnosti. Nijedno nije težak prekršaj, ali sva su lako popravljiva
i mentor ih tipično traži.

### 4.1 Slike preuzete iz literature bez navedenog izvora 🔴

Rad ispravno navodi izvor kod `Slike 2.3` [4], `3.3` [11], `3.5` (Cohen i sur.) i `4.2` [14].
**Kod sljedećih izvor nedostaje**, iako su očito preuzete:

| Slika | Sadržaj | Vjerojatan izvor |
|---|---|---|
| `Slika 2.1` | Interakcija agenta s okolinom | Sutton & Barto, odnosno [4] |
| `Slika 2.2` | Načini modeliranja vrijednosti / politike / okoliša | [4] |
| `Slika 3.1` | Pseudokod algoritma PPO | **Schulman i sur. [9]** — pseudokod je doslovno preuzet iz rada |
| `Slika 3.2` | Usporedba algoritama na MuJoCo okruženjima | **Schulman i sur. [9]** |
| `Slika 3.4` | Prikaz četiriju testnih okruženja | **Cohen i sur. [11]** |

Reproduciranje objavljene slike zahtijeva navođenje izvora u naslovu slike. Zahvat: dodaj
„Preuzeto iz: [9]" odnosno „[11]" na kraj naslova, kao što je već učinjeno kod `Slike 3.3`.

### 4.2 ELO sustav — formule bez izvora 🔴

Potpoglavlje 3.5.3 iznosi ELO sustav („izvorno razvijen za rangiranje šahista"), uključujući
**formulu očekivanog rezultata i formulu ažuriranja ocjene**, bez ijedne reference. Riječ je o tuđem
sustavu i tuđim izrazima.

Zahvat: citiraj Elovu monografiju — *Elo, A. E.: „The Rating of Chessplayers, Past and Present",
Arco Publishing, New York, 1978.* — pri prvom spominjanju i uz formule. Ako se koristi ML-Agents
implementacija ELO-a, dodaj i uputu na [16] (dokumentacija samoigre).

### 4.3 Podrijetlo pojma „oblikovanje nagrade" — bez izvora 🟡

Potpoglavlje 4.6.1 tvrdi da je pojam „izravno posuđen iz bihevioralne psihologije i dresure
životinja, gdje je pokazano da se složeno ponašanje ne uči nagrađivanjem isključivo konačnog cilja,
nego nagrađivanjem niza uzastopnih približavanja (eng. „successive approximations")".

„Successive approximations" je **Skinnerov termin** iz operantnog uvjetovanja i tvrdnja „gdje je
pokazano" izravno se poziva na tuđe eksperimentalne rezultate. Zahvat: citiraj Skinnera
(*Skinner, B. F.: „The Behavior of Organisms", Appleton-Century, New York, 1938.*) ili noviji
pregledni rad o shapingu u RL-u koji tu vezu uspostavlja (npr. Ng, Harada i Russell [12], koji je
već u popisu).

### 4.4 Tvrdnje iz teorije progona i bijega — bez izvora 🟡

Na dva mjesta rad se poziva na „teoriju progona i bijega" kao na izvor tvrdnje:

- 3.4.6: „progonitelj s jednakom brzinom može pobijediti jedino presjekom puta ili tjerajući
  protivnika u kut"
- 6.5.4: „u teoriji progona i bijega, zaklonište pomaže bjeguncu ako ga može doseći prije
  progonitelja"

Obje su tvrdnje preuzete iz područja (pursuit-evasion / diferencijalne igre) i **nose zaključak
RQ-C**, pa je izostanak izvora ovdje najuočljiviji. Zahvat: citiraj temeljni izvor —
*Isaacs, R.: „Differential Games", John Wiley & Sons, New York, 1965.* — ili noviji pregledni rad o
problemima progona i bijega.

### 4.5 „Stalni član" kao vlastiti nalaz — provjeriti 🟡

Potpoglavlje 3.4.6 kaže: *„Ključan i kontraintuitivni zaključak **analize ovog istraživanja** jest
da ovaj doprinos raste s udaljenošću."* Sam izvod jest vlastiti i to je legitimno. Međutim, **općenit
nalaz da invarijantnost PBS-a slabi kada je γ < 1, odnosno u epizodnim zadacima, postoji u
literaturi** (npr. Wiewiora, 2003.; Grześ, 2017.).

Ovo nije prekršaj, ali je taktički propust: tvrdnja djeluje jače ako se pokaže da je poznato
ograničenje prepoznato u literaturi, a da je **doprinos ovog rada kvantifikacija tog člana u
konkretnom zadatku i njegovo povezivanje s farming-zamkom**. Preporuka: zadrži formulaciju o
vlastitoj analizi, ali dodaj rečenicu koja priznaje raniji rad i citira ga.

### 4.6 Sitno — navod dokumentacije 🟢

Doslovan navod iz ML-Agents dokumentacije u 5.4 ispravno je u navodnicima i ima referencu [13].
Jedini nedostatak je što je ostao na engleskom; Upute traže hrvatski prijevod uz izvorni navod.

---

## 5. Grafika — preostalo

Slike su unesene (32 ukupno). **Preostali `TODO` markeri:**

| Mjesto | Grafika | Status |
|---|---|---|
| 3.4.6 | Graf potencijalne funkcije Φ(s) + stalnog člana koji raste s udaljenošću | 🔨 treba izraditi |
| 4.2 | Tortni dijagram profila vremena (env_step 78 %, inference 14 %, gradijenti 6 %) | 🔨 treba izraditi |
| 4.4 | Shematski tlocrt arene 20×20 s raycast zrakama | 🔨 treba izraditi |
| 5.4 | Dijagram toka nagrade MA-POCA vs PPO | 🔨 **najveća vrijednost za uloženi trud** — nosi objašnjenje zamke |

---

## 6. Preporučeni redoslijed rada

**Prvo (najveći učinak, uglavnom brisanje):**

1. Provedi zahvate iz **`DUPLIKATI_u_poglavljima_5_i_6.md`** — uklanja dvostruki tekst u 5.5,
   premješta rezultate u poglavlje 6 i usput rješava prazne jednadžbe i dvije pokvarene upute.
2. **Prepiši Uvod** — izbaci Taxi/Kafka odlomke, popravi „Error! Reference source not found.".

**Drugo (atribucija, ~1 h):**

3. Dodaj izvore slikama 2.1, 2.2, 3.1, 3.2, 3.4 (§4.1).
4. Citiraj Ela uz ELO formule, Skinnera uz podrijetlo shapinga, Isaacsa uz tvrdnje o progonu i
   bijegu (§4.2–4.4).
5. Dodaj rečenicu o ranijem radu uz „stalni član" (§4.5).

**Treće:**

6. Ispravi brojke o arenama (277 vs 495 → 553).
7. Numeriraj formule; preurediti literaturu u FESB format; poglavlje 2 u pasiv; prevedi naslove
   6.4.1 i 6.4.2.
8. Izradi preostale četiri slike.
9. **Na kraju:** Zaključak, Sažetak/Abstract, ključne riječi, Dodatak A.
