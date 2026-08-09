# Evaluacija diplomskog rada — revizija 2 (6.8.2026.)

Analizirano: glavni `.docx` (392 odlomka, 14 slika, **5 tablica**, 30 jednadžbi),
`Theory.md`, i izvedeni `Teorijski i empirijski dio … MA-POCA Tag.md`.
Prethodna evaluacija: 5.8.2026. (341 odlomak, 13 slika, 2 tablice).

> **Napomena o opsegu:** Faza B je izbačena iz plana rada. Sve preporuke ispod uzimaju to u
> obzir — Faza B se spominje samo kao *budući rad*, nikad kao dio istraživanja.

---

## 1. Što je napravljeno od prošle evaluacije

| Dodano | Ocjena |
|---|---|
| **Novo poglavlje 6 „Empirijski rezultati"** s potpoglavljima 6.1–6.4 | ✅ Najveći pomak. Rad sada ima rezultate. |
| Tablica 3 — `PolicyLoss / ValueLoss / BaselineLoss` (dokaz da je trener POCA) | ✅ Točno preneseno iz `Theory.md` §2 |
| Tablica 4 — sparse ruka, 5M, 3 sjemena | ✅ |
| Tablica 5 — shaped ruka, 5M, 3 sjemena | ✅ (nedostaje naslov tablice) |
| Potpoglavlje 6.2 — sažetak validacije na 400k | 🟡 Tekst da, tablica ne |
| Potpoglavlje 6.4 — rasprava u 4 dijela | ✅ Dobro strukturirano, prati treći dokument |
| Slika 4-1 — blok dijagram ML-Agents Toolkita + referenca [14] | ✅ Prva slika u vlastitom poglavlju |
| Ispravljen odlomak o farming-zamki (mehanizam, nomenklatura, tri razloga) | ✅ |

**Pokrivenost `Theory.md` porasla je s ~40 % na ~60 %.** Rad je prešao iz stanja „nema
rezultata" u stanje „ima rezultate, ali nedosljedno organizirane".

---

## 2. Iskreno mišljenje

**Dobro:** poglavlje 6 je pravi napredak i po sadržaju i po tonu. Rasprava (6.4) je najbolje
napisan dio cijelog rada — argumentacija o dva uzroka PBS zamke, o γ platou i o negativnom
RQ-C rezultatu drži razinu koja se očekuje od diplomskog rada i iznad. Poanta o inverziji
400k → 5M sada je jasno izrečena i to je ono što rad izdiže iznad „reprodukcije YouTube videa".

**Loše, i to je sada glavni problem:** *organizacija je proturječna sama sebi.* Konkretno:

1. **„Faza A" znači dvije različite stvari u istom radu.** Poglavlje 5.5 nosi naslov
   „**Faza A**: fiksne prepreke, 9 konfiguracija" i opisuje gamma pretraživanje s preprekama.
   Poglavlje 6 nosi naslov „Empirijski rezultati – **FAZA A**", ali sadrži smoke-test,
   validaciju na 400k i sparse-vs-shaped pokrete na 5M — **od kojih nijedan nije Faza A**.
   Faza A je, po `Theory.md` §14, posebni eksperiment s 9 pokreta koji dolazi *nakon* svega
   u poglavlju 6. Čitatelj (i mentor) ovo neće moći pomiriti.
2. **Rasprava raspravlja o podacima koji nisu prikazani.** Poglavlje 6.4.2 navodi „0,12
   naspram PPO-ovih 0,98 stope hvatanja" — ta brojka dolazi iz sonde kanalske isporuke
   (`Theory.md` §13) koja se **nigdje u radu ne prikazuje**. Isto vrijedi za 6.4.3, koje
   raspravlja krivulju osjetljivosti γ, a tablica s 9 pokreta ne postoji. Ne možeš raspravljati
   o rezultatu koji nisi pokazao.
3. **Rezultati su na dva mjesta.** Poglavlje 5 (dizajn) i dalje sadrži „Zaključak eksperimenta"
   u 5.4 i cijele RQ-A/RQ-C nalaze u 5.5, dok poglavlje 6 sadrži druge rezultate. Čitatelj
   dobiva rezultate gamma pretraživanja *prije* rezultata osnovnog sparse-vs-shaped pokusa —
   obrnuto od kronologije i od logike.
4. **Naslov 6.3.2 proturječi vlastitom tekstu.** Naslov glasi „Shaped arm – **„proximity
   farming"** – inverzija rezultata", a odlomak ispod eksplicitno kaže da naziv *proximity
   farming* nije točan i da je ispravan naziv *shaping-farming*. Naslov mora slijediti ispravak.

**Sažetak:** sadržaj je sada dobar; problem više nije *što* je napisano nego *gdje*. Sljedeći
korak nije pisanje novog teksta nego preslagivanje — to je jeftinije nego što izgleda i podiže
dojam rada više nego bilo koji novi odlomak.

---

## 3. Novi problemi uvedeni ovom revizijom

| # | Problem | Prijedlog |
|---|---|---|
| N1 | „Faza A" ima dva značenja (v. gore) | Ukloni „FAZA A" iz naslova poglavlja 6 → **„6 Empirijski rezultati"**. Budući da Faza B otpada, oznaka faze više ničemu ne služi. U 5.5 zadrži opisni naslov: „Pretraživanje γ u areni s fiksnim preprekama". |
| N2 | Uvodni odlomak poglavlja 6 objašnjava Fazu A vs Fazu B | Faza B se ne piše — preseli to objašnjenje u *Budući rad* u jednoj rečenici. |
| N3 | Naslov 6.3.2 kaže „proximity farming", tekst ga opovrgava | → „6.3.2 Oblikovana ruka — oblikovno farmiranje (*shaping-farming*) i inverzija rezultata" |
| N4 | 6.4.2: „farming-zamka oblikovanog **(sparse)** Chasera" | Treba **(shaped)**. Kritična zamjena baš u ključnoj rečenici rada. |
| N5 | Tablica 5 (shaped ruka) nema naslov tablice | Dodaj „Tablica 5 …"; Tablica 4 se zove „Rezultati 5M treninga FAZE A" iako pokriva samo sparse ruku → preimenuj u „Rezultati rijetke (*sparse*) ruke, 5M". |
| N6 | 6.4.2 i 6.4.3 citiraju podatke koji nisu prikazani (PPO 2×2, sonda isporuke, γ krivulja) | Ili dodaj tablice T9–T12 (v. §5), ili skrati raspravu na ono što je prikazano. **Preporuka: dodaj tablice — podaci su gotovi u `Theory.md`, prijenos je ~1 h.** |
| N7 | Slika u 6.3 (ispod naslova „Glavni rezultati") bez naslova i broja | Dodaj `Caption` sa `SEQ Slika` da uđe u Kazalo slika. |
| N8 | 6.2 se zove „Metrike: stopa hvatanja, duljina epizode, ELO", a sadrži rezultate validacije na 400k | Preimenuj u „6.2 Validacijski rezultati (400k koraka)". Prave *definicije* metrika idu u poglavlje 5 (v. §4). |
| N9 | Tablice 4 i 5 prikazuju ELO „tima koji je u tom trenutku trenirao" | Bez objašnjenja izgleda kao greška (sparse s1 = Chaser 1890, s2 = Runner 685). Dodaj rečenicu ili fusnotu: u samoigri konzola prijavljuje tim koji uči, pa niski Runner ELO znači da Chaser dominira. |

---

## 4. Gdje objasniti samoigru, metrike i ostalo

Ovo je izravan odgovor na tvoje pitanje. Redoslijed je bitan: svaki pojam mora biti definiran
**prije** prve upotrebe.

| Sadržaj | Preporučeno mjesto | Obrazloženje |
|---|---|---|
| **Samoiga (self-play) i ELO** — snapshot protivnici, `team_change`, „Not Training" tim, ELO od 1200, zašto brojač koraka prekorači budžet | **Novo poglavlje 3.5**, odmah nakon 3.4 (MA-POCA) | Samoiga je *metoda učenja*, ravnopravna s PPO i MA-POCA, i nije specifična za tvoju implementaciju. Trenutno se spominje 8+ puta prije nego što je ijednom objašnjena, a cijelo poglavlje 6 počiva na ELO-u. **Ovo je najveća pojedinačna rupa u radu.** Izvor: `Theory.md` §1, §3. |
| **Konkretni parametri samoigre** (`save_steps`, `swap_steps`, `team_change`, `play_against_latest_model_ratio`, inicijalni ELO) | **4.6 Konfiguracija treniranja** | Standardna podjela: teorija u pogl. 3, parametrizacija u pogl. 4. |
| **Mjerne veličine** — što je `Environment/Catch`, `TimeToCatch`, `Episode Length`, `Group Cumulative Reward` vs individualna nagrada, `Entropy`, `BaselineLoss`; **koje su metrike neovisne o oblikovanju** | **Novo poglavlje 5.2**, između „Istraživačka pitanja" i „Eksperiment 1" | Mora doći **prije** 5.2.1 (a priori kriteriji uspjeha), jer ti kriteriji već koriste catch rate, duljinu epizode i ELO. Bez ovoga cijela usporedba sparse vs shaped nije obranjiva — ključna je poanta da se ruke smiju uspoređivati samo po grupnoj nagradi, ELO-u i stopi hvatanja, **ne** po individualnoj nagradi. Izvor: `Theory.md` §7. |
| **Polazišna crta slučajne politike** (ep. length ≈ 393/380, catch 5–15 %, kum. nagrade −1,97/+1,90, entropija 1,42) | **Novo poglavlje 6.1**, na samom početku rezultata | To je „korak 0" trke u naoružanju i referentna točka za svaki kasniji broj. Trenutno se pojavljuje samo posredno, kao prag u kriterijima uspjeha. Izvor: `Theory.md` §4. |
| **Hardver** (i7-9750H 6c/12t, 16 GB, GTX 1660 Ti, CPU PyTorch) | **4.6 Konfiguracija treniranja**, uz tablicu | Nigdje se ne spominje, a bez toga tvrdnja „GPU je irelevantan" visi u zraku. Izvor: `Theory.md` §10. |
| **Odbačene alternative oblikovanju** (naivna distance-delta nagrada; prednost brzine 6/5) i zašto je odabran PBS | **3.4.6**, prije „Oblikovanje zasnovano na potencijalu" | Pokazuje da izbor nije proizvoljan. Trenutno se 6/5 spominje tek kao „rezervni scenarij" u 5.2.1, bez konteksta. Izvor: `Theory.md` §6. |
| **`individual_terminal_reward` i smoke-validacija PPO ruke** | **5.3**, gdje je već motivacija | Objašnjava zašto usporedba PPO-a nije nepoštena. Izvor: `Theory.md` §13. |
| **Ograničenja i prijetnje valjanosti** (1 sjeme na unutarnjim γ točkama, PPO 1 sjeme, ELO je relativan, sve je 1v1, implicirana udaljenost je izvedena a ne mjerena) | **Novo poglavlje 6.5**, na kraju rasprave | Standardno mjesto; jača dojam ozbiljnosti. Izvor: `Theory.md` §15 / treći dok. §6.5. |
| **Budući rad** (Faza B, timska ekspanzija 2v1/2v2, sweep gustoće prepreka, prag coef-a, seed-hardening, kvalitativna taksonomija) | **Novo poglavlje 7**, prije Zaključka | Ovdje Faza B pripada — kao plan, ne kao provedeni rad. Izvor: `Theory.md` §15. |
| **Isječci koda** (Φ(s) i F u `TagAgent`, `AddGroupReward` + `EndGroupEpisode`, YAML `poca` + `self_play`, `StatsRecorder`) | 4.5, 4.6 i 3.4.6 | „Kazalo kodova" postoji, a u radu nema nijednog listinga. 4–6 kratkih isječaka je dovoljno. |

---

## 5. Tablice koje još nedostaju (podaci su gotovi)

| # | Tablica | Mjesto | Izvor |
|---|---|---|---|
| T4 | Profil vremena treniranja (faza, s, udio) | 4.2 | `Theory.md` §5 |
| T5 | Popis logiranih metrika i njihovo značenje | novo 5.2 | §7 |
| T6 | Hardver + konfiguracija treniranja (mreža 256×2, γ, LR, 16 arena, batch…) | 4.6 | §10 |
| T7 | **Validacija 400k: sparse vs shaped**, 9 metrika | 6.2 | §11 |
| T9 | **Matrica 2×2** POCA/PPO × sparse/shaped | novo 6.4 (rezultati, ne rasprava) | §13 |
| T10 | **Sonda kanalske isporuke** (grupni / grupni+individualni / individualni) | novo 6.4 | §13 |
| T11 | Gamma sonde RQ-B (γ = 0,8 / 0,9 / 0,99) + omjer žetve 1 : 8,1 : 19,2 | novo 6.5 | §14 |
| T12 | **Faza A — 9 pokreta** (γ, sjeme, catch, ELO, razmak, ep. length, TimeToCatch, GroupR) | novo 6.6 | §14 |
| T13 | Sumarno: RQ → predikcija → ishod (potvrđeno / odbačeno) | kraj poglavlja 6 | §14, §15 |
| T14 | Ograničenja i prijetnje valjanosti | 6.7 | §15 |

**T12 je najhitnija** — trenutno postoji rasprava o γ krivulji bez ijednog broja koji je potkrepljuje.

---

## 6. Popis mjesta gdje bi grafika bila prikladna

Poredano po poglavljima. **P** = prioritet (1 = obavezno, 3 = ako ostane vremena).
**Status:** ✅ postoji · 🔨 treba izraditi · 📁 postoji u repozitoriju, treba samo umetnuti.

### Poglavlje 3 — teorija

| P | Mjesto (sidro u tekstu) | Grafika | Status |
|---|---|---|---|
| 2 | **3.4.3**, uz „…primijeni mehanizam samopažnje (self-attention)" | Shema arhitekture MA-POCA kritičara: koderi opažanja → RSA blok → vrijednosna glava i kontrafaktična baza | 🔨 vlastiti dijagram |
| 1 | **3.4.6**, postojeći `TODO` | Graf potencijala Φ(s) po udaljenosti **+ drugi graf stalnog člana (1−γ)·coef·(d/maxDist)** koji raste s udaljenošću | 🔨 trivijalno u matplotlibu |
| 2 | **novo 3.5** (samoiga) | Dijagram petlje samoigre: trenirajući tim ↔ snapshot protivnik, `save_steps`, `team_change`, ELO ažuriranje | 🔨 vlastiti dijagram |

### Poglavlje 4 — okruženje i implementacija

| P | Mjesto | Grafika | Status |
|---|---|---|---|
| — | 4.1 | Blok dijagram ML-Agents Toolkita | ✅ Slika 4-1 |
| 1 | **4.2**, postojeći `TODO` | Tortni ili horizontalni stupčasti dijagram profila vremena: `env_step` 78 % (od toga `communicator.exchange` 42 %), inference 14 %, gradijenti 6 % | 🔨 iz `timers.json` |
| 1 | **4.3**, uz opis igre | Screenshot arene iz Unityja s označenim Chaserom, Runnerom, zidovima i 4 stupa | 🔨 screenshot |
| 1 | **4.4**, postojeći `TODO` | Shematski tlocrt arene 20×20: agenti, zidovi, primjer raycast zraka, oznaka 18-dim vektora opažanja | 🔨 vlastiti dijagram |
| 2 | **4.5**, uz Tablicu 1 | Vremenska crta epizode: početak → koraci → hvatanje (`EndGroupEpisode`) ili timeout (`GroupEpisodeInterrupted`), s ucrtanim točkama dodjele nagrada | 🔨 |
| 1 | **4.6**, postojeći `TODO` | Screenshot Unity Editora, Scene_V2, 16 arena na X-osi, s oznakom razmaka ≥35 j. | 🔨 screenshot |
| 2 | **4.6**, uz bake-off brojke | Stupčasti graf propusnosti: 4 / 12 / 16 arena (277 / 495 / 553 koraka/s) + druga os „koraci/s po areni" (69 / 41 / 35) | 🔨 |
| 3 | **4.6.1**, postojeća slika bez naslova | Provjeriti što je i dodati naslov, ili ukloniti | ⚠️ |

### Poglavlje 5 — eksperimentalni dizajn

| P | Mjesto | Grafika | Status |
|---|---|---|---|
| 1 | **5.3**, postojeći `TODO` | **Dijagram toka nagrade MA-POCA vs PPO**: grupni kanal → centralizirani kritičar, nasuprot individualnom `Agent.AddReward`; strelice pokazuju gdje PBS ulazi. Nosi cijelo objašnjenje zamke. | 🔨 |
| 2 | **5.1**, uz RQ-A/B/C | Shema matrice svih eksperimenata: {sparse, shaped} × {POCA, PPO} × γ, s brojem sjemena po ćeliji — jedan pregled cijelog programa od 20 pokreta | 🔨 |

### Poglavlje 6 — rezultati

| P | Mjesto | Grafika | Status |
|---|---|---|---|
| 2 | **novo 6.1** (polazišna crta) | Krivulje `Episode Length` i `Catch` za prvih 50–100k koraka s ucrtanim pragovima (393 koraka / ~10 %) | 📁 |
| 1 | **6.1/6.2**, postojeći `TODO` | TensorBoard: `BaselineLoss` uz `ValueLoss` za oba agenta — vizualni dokaz da je trener POCA | 📁 |
| 1 | **6.2**, postojeći `TODO` | 400k validacija: ELO divergencija (Fig. 2) + catch rate i duljina epizode (Fig. 3) | 📁 `figures/validation/` |
| **1** | **6.3**, slika bez naslova | **Glavna slika rada:** sparse vs shaped kroz 5M, 3 sjemena, min–max pojas; 3 panela: ELO, `Group Cumulative Reward`, individualna `Cumulative Reward` (koja pokazuje farmiranje) | 📁 |
| 2 | **6.3.2**, uz opis farmiranja | Sličice iz Editor-inferencije: sparse Chaser presijeca put vs shaped Chaser loiteri na sredini arene — 2×3 mreža sličica | 🔨 snimka zaslona |
| 1 | **novo 6.4** (PPO 2×2) | Fig. 5 — stopa hvatanja za sve 4 ćelije; Fig. 6 — ELO Chasera. Jedna ćelija (POCA+shaped) ostaje na 1 % | 📁 `figures/ppo/` |
| 1 | **novo 6.4** (sonda isporuke) | Fig. 7 — 3 krivulje stope hvatanja: samo grupni (~0,01) / grupni+individualni (~0,12, još raste) / samo individualni (0,98) | 📁 `figures/ppo/` |
| 1 | **novo 6.5** (γ sonde) | Fig. 8 — catch pod 1 % za sve γ + „ljestve žetve" individualne nagrade (+122,8 / +50,8 / +4,5) na istom grafu | 📁 `figures/gamma/` |
| **1** | **novo 6.6** (γ sweep) | Fig. 9 — krivulje učenja po γ s min–max pojasom; **bimodalnost γ=0,995 vidljiva je isključivo grafički** | 📁 `figures/gamma/` |
| 1 | **novo 6.6** | Fig. 10 — krivulja osjetljivosti: konačna stopa hvatanja i ELO razmak u ovisnosti o γ, s pojedinačnim sjemenima kao točkama | 📁 |
| 2 | **novo 6.6** | Fig. 11 — `Self-play/ELO` svih 9 pokreta; jasno se vidi `g0995_s1` kao ravna linija do ~4,3M | 📁 |
| 3 | **kraj 6** | Sumarna grafika: RQ-A/B/C → predikcija → ishod (potvrđeno/odbačeno), kao vizualna tablica | 🔨 |

**Ukupno: 8 slika je već izrađeno i čeka samo umetanje (📁), 13 treba izraditi (🔨).**
Od onih koje treba izraditi, najveći omjer dojma i uloženog vremena imaju: profil vremena (4.2),
tlocrt arene (4.4), screenshot 16 arena (4.6) i dijagram toka nagrade (5.3).

---

## 7. Neriješeno od prošle evaluacije

Sve niže navedeno i dalje stoji nepromijenjeno:

1. **Uvod, odlomci 4–9** i dalje govore o *Taxi aplikacijama, Apache Kafki, WebSocketu, Firebaseu
   i FUBAR Taxiju*. Šest odlomaka iz tuđeg rada. **Najveći rizik za dojam pri predaji.**
   Također: „Rad je podijeljen na sedam poglavlja" — sada ih je 6 + zaključak.
2. **5.5, prva rečenica je i dalje pokvarena:** „…po jedan pokret za te tri sjemena (seeds) za
   γ=0,995. / γ=0,995, svaki treniran kroz 5M koraka". Nedostaje popis γ ∈ {0,8; 0,9; 0,95;
   0,99; 0,995} i objašnjenje matrice (3 sjemena na rubovima, 1 u sredini = 9).
3. **5.4:** „u sve tri testirane konfiguracije (; ; )" — vrijednosti γ nedostaju u zagradama.
   Provjeri jesu li to prazne jednadžbe (isti problem u 5.4: „prema formuli ." i „dok pri  pada").
4. **Netočnost:** „stopa hvatanja rasla je s 0,86 pri γ=0,8 do 1,00 **pri γ=0,99**" — po
   `Theory.md` §14 vrhunac stope hvatanja (1,00) je na **γ=0,95**; na γ=0,99 je 0,99. γ=0,99 se
   brani *brzinom učenja* i duljinom epizode, ne najvišom stopom.
5. **Netočnost:** „povećanje broja arena od 4 na 16 → sa ~277 na ~553" spaja dva odvojena
   mjerenja. Bake-off je bio **12 arena = 495 vs 16 arena = 553 (+12 %)**; 277 je iz smoke-testa
   s 4 arene.
6. Nedosljednost 78 % (4.2) vs „~80 %" (4.6.1).
7. **Tablica 1**, ćelija PBS/Chaser i dalje prazna → `F = γΦ(s′) − Φ(s)`.
8. **Tablica 2**: „JEDANKI" → „JEDNAKI".
9. Referenca **[7]** (Baker) citirana uz tvrdnju o ML-Agents Toolkitu; ondje pripada samo [8].
10. Naslov poglavlja 3: „Pregled korištenih **rml** Algoritama" — nedosljedna kapitalizacija.
11. **Popis oznaka i kratica** prazan (~25 kratica u radu).
12. **Kazalo tablica / kodova** prikazuju „No table of figures entries found" — polja treba
    osvježiti (Ctrl+A → F9). Sada kada postoji 5 tablica, ovo će konačno imati sadržaj.
13. Slika odmah ispod naslova „LITERATURA" — provjeriti i ukloniti.
14. **Zaključak, Sažetak, Abstract, ključne riječi** — i dalje prazni.
15. Visjeća referenca `[pogl. X.Y]` u 6.3.2 (moj placeholder) → zamijeni brojem poglavlja s
    gamma sondama čim struktura bude fiksna.

---

## 8. Preporučeni redoslijed rada

**Prvo (pola dana, najveći učinak):**

1. Prepisati uvod — izbaciti Kafka/Taxi tekst, ispraviti broj poglavlja.
2. Ukloniti „FAZA A" iz naslova poglavlja 6 i preurediti uvodni odlomak (N1, N2).
3. Ispraviti N3, N4, N5, N8 — sve su jednorečenične izmjene u ključnim rečenicama.
4. Preseliti rezultate iz 5.4 i 5.5 u poglavlje 6; u poglavlju 5 ostaviti samo dizajn i predikcije.

**Drugo (jedan dan):**

5. Dodati 3.5 Samoiga, 5.2 Mjerne veličine, 6.1 Polazišna crta — tri rupe koje najviše smetaju
   čitatelju.
6. Prenijeti tablice T7, T9–T12 iz `Theory.md` (čisti prijenos brojeva).
7. Umetnuti 8 već izrađenih slika (📁).

**Treće:**

8. Izraditi grafike označene 🔨 prioritetom 1.
9. Napisati 6.7 Ograničenja, poglavlje 7 Budući rad (uklj. Fazu B kao plan), Zaključak, Sažetak/
   Abstract, popis kratica.
10. Ispraviti brojčane netočnosti (t. 4–9 iz §7), ukloniti sve `TODO` oznake, Ctrl+A → F9.
