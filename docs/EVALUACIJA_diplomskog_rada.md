# Evaluacija diplomskog rada — revizija 3 (13.8.2026.)

Analizirano: glavni `.docx` (**725 odlomaka, 27 slika, 8 tablica, 3 isječka koda, ~30 jednadžbi**)
i `Theory.md`. Prethodne evaluacije: 6.8.2026. (392 odlomka, 14 slika, 5 tablica), 5.8.2026.
(341 odlomak, 13 slika, 2 tablice).

> **ISPRAVAK opsega (važno).** Revizija 2 je tvrdila da je **Faza B izbačena iz plana rada**. To je
> bilo **netočno**. Faza B je **odgođena, nije otkazana**: redoslijed je dovršiti teoriju i pisanje za
> Fazu A i sve prije nje, pa *zatim* pokrenuti Fazu B (rebuild + `TagMApoca_obs_smoke` gate ostaje
> prva radnja pri povratku na eksperimente). Stavke N1/N2 iz revizije 2 („Faza B otpada / se ne piše")
> ovime se povlače. Posljedica za tekst: Faza B se **smije** najavljivati kao sljedeći korak, ali
> nigdje ne smije zvučati kao da su njezini rezultati već dobiveni.

---

## 1. Što je napravljeno od prošle evaluacije

| Dodano | Ocjena |
|---|---|
| **17 slika u poglavlju 6** (6.1–6.17): 5 × validacija 400k, 6 × sparse 5M, 6 × shaped 5M | ✅ Najveći pomak. Rad je prešao iz „ima rezultate, ali ih ne pokazuje" u „pokazuje ih sustavno". |
| **Tablica 5** — 400k validacija, sparse vs shaped | ✅ Rješava staru zamjerku N6 (rasprava je citirala nepokazane podatke) |
| **Tablica 6 i 7** — rezultati sparse i shaped ruke na 5M | ✅ Obje s naslovom **iznad** tablice, kako Upute traže |
| **γ-tablica Faze A** (9 pokreta) u §6.4.3 | ✅ Sadržajno najhitnija stavka iz prošle evaluacije (T12) — ali bez naslova i s markdown artefaktima (v. §3) |
| **Popis oznaka i kratica** — 25 stavki, abecedno | ✅ Riješeno u cijelosti |
| **Tri isječka koda** + funkcija `OnAgentTagged` | ✅ Rad koji je u cijelosti programski konačno pokazuje kod |
| **Parametri samoigre popunjeni** (50 000 / 50 000 / 100 000; validacija 25 000/25 000/50 000) | ✅ Uklonjeni `⟨…⟩` placeholderi i „EXPLAIN" bilješka |
| **Novo potpoglavlje „Scena"** + `Slika 4.2` (16 arena) | ✅ Riješen jedan od `TODO` markera |
| **Numeracija slika po poglavljima**, naslovi poglavlja velikim slovima | ✅ Dvije formalne zamjerke riješene odjednom |
| Ispravak: vrhunac stope hvatanja je **γ=0,95**, ne γ=0,99 | ✅ Točan podatak, usklađen s `Theory.md` §14 |
| Ujednačeno 78 % (bilo 78 % / ~80 %) | ✅ |

**Pokrivenost `Theory.md` porasla je s ~60 % na ~85 %.** Ostaje neprikazan samo eksperiment 2
(MA-POCA vs PPO) — v. §2, točka 2.

---

## 2. Iskreno mišljenje

**Dobro, i bolje nego prošli put.** Poglavlje 6 sada radi ono što treba: iznosi tvrdnju, pokaže
tablicu, pokaže graf. Inverzija 400k → 5M je ispričana s podacima na obje strane, a rasprava
(§6.4) je i dalje najjači dio rada — argumentacija o dva uzroka PBS zamke i o γ platou drži razinu
iznad prosjeka diplomskog rada. **Provjerio sam svaki broj u Tablicama 5, 6, 7 i u γ-tablici protiv
`Theory.md` — svi se poklapaju. Nema izmišljenih podataka**, što je pri ovoj količini prenesenih
brojeva vrijedno spomena.

**Tri stvari koje sada najviše smetaju:**

1. **Uvod je i dalje tuđi.** Odlomci 115–120 govore o Taxi aplikacijama, Apache Kafki, WebSocketu,
   Firebaseu i „FUBAR Taxiju". To stoji nepromijenjeno kroz tri evaluacije. Uz to, odlomak 113 ima
   vidljivo **„Error! Reference source not found."**. Rad sada ima ozbiljno poglavlje 6 — a mentor
   će prvo pročitati uvod i vidjeti Kafku. **Ovo više nije stilska zamjerka nego rizik za dojam o
   cijelom radu**, i nije vezano ni uz Fazu B ni uz išta odgođeno: može se izbaciti danas.

2. **Eksperiment 2 (MA-POCA vs PPO) i dalje nema rezultate.** §5.4 opisuje dizajn matrice 2×2, ali
   nigdje u poglavlju 6 nema njezinih ishoda. Istovremeno §6.4.2 tvrdi: *„MA-POCA ostaje zarobljeno
   u zamci s 0,12 naspram PPO-ovih 0,98 stope hvatanja"* — brojka koja se **nigdje ne prikazuje**.
   To je jedina preostala instanca stare zamjerke „rasprava raspravlja o podacima koji nisu
   prikazani", i sada strši jer su svi ostali slučajevi riješeni. Podaci su gotovi u `Theory.md` §13;
   prijenos su dvije male tablice (gotove u `VODIC_ZA_DOVRSETAK_RADA.md` §4).

3. **Rezultati Faze A i dalje su na dva mjesta.** §5.6 („Faza A: fiksne prepreke") sadrži prozu s
   brojkama (0,86 → 1,00, bimodalnost na 0,995, negativan RQ-C), a §6.4.3 sadrži istu priču plus
   tablicu. Poglavlje 5 treba biti dizajn i predikcije; brojke pripadaju u 6. Ovo je jedina veća
   organizacijska zamjerka koja je preživjela iz revizije 2.

**Sažetak:** rad je sadržajno gotov do razine Faze A. Ono što ga sada drži nedovršenim nije
istraživanje nego **čišćenje** — uvod, jedna nedostajuća tablica rezultata i mehanika (kazala,
numeracija, tipfeleri).

---

## 3. Novi problemi uvedeni ovom revizijom

Sve nastalo pri kopiranju iz markdowna u Word. Brzo se popravlja, ali se **vidi na prvi pogled**:

| # | Problem | Popravak |
|---|---|---|
| N1 | γ-tablica sadrži **markdown zvjezdice**: ćelija piše `**1,00**` | Obriši `**`, koristi Wordov bold |
| N2 | γ-tablica **nema naslov ni broj** — jedina takva u radu, neće ući u Kazalo tablica | Dodaj naslov iznad: „Tablica 6.5. Rezultati Faze A — devet konfiguracija" |
| N3 | **Tablica 5 nema redak sa stopom hvatanja** — a to je glavna ishodna metrika, tekst je spominje i `Slika 6.1` je prikazuje | Dodaj redak `Environment/Catch`: sparse `~0,08`, shaped `~0,21` |
| N4 | Tablica 5 koristi **decimalne točke**, Tablice 6/7 zareze | Ujednači na zarez |
| N5 | **15 naslova bez razmaka iza broja**: `Slika 6.1Stopa…`; najgore `Tablica 5400k validacija` | Dodaj razmak |
| N6 | **Tipfeleri u naslovima slika**: `Envirnoment` (4×), `Lenght` (2×), `Culmutive`, `Cumultive`, `Rezultait`, `objeruke` (2×), `grafovasnimljenih` | Ispravi |
| N7 | Tablice numerirane **linearno** (1–7), slike **po poglavljima** (6.17) — rad nedosljedan sam sa sobom | Prenumeriraj tablice po poglavljima |
| N8 | `Kod 31`, `Kod 41`, `Kod 42` — bez točke, ne prate shemu slika | → `Kod 3.1`, `4.1`, `4.2` |
| N9 | **Sva tri kazala su zastarjela** — Kazalo slika prikazuje 9 starih stavki (`Slika 21`…`41`) umjesto 27; Kazalo kodova i dalje piše „No table of figures entries found" iako kodovi postoje | `Ctrl+A` → `F9` |

---

## 4. Neriješeno od prošle evaluacije

| # | Stavka | Status |
|---|---|---|
| 1 | **Uvod, odlomci 115–120** — Taxi/Kafka/WebSocket/Firebase tekst | 🔴 nepromijenjeno kroz tri evaluacije |
| 2 | „Rad je podijeljen na **sedam** poglavlja" — stvarno 6 + Zaključak | 🟡 namjerno odgođeno dok se struktura ne zaključa |
| 3 | §5.5, prva rečenica i dalje pokvarena: nedostaje popis γ ∈ {0,8; 0,9; 0,95; 0,99; 0,995} i objašnjenje matrice (3 sjemena na rubovima, 1 u sredini = 9) | 🔴 |
| 4 | §5.5: **prazne jednadžbe** — „prema formuli **.**", „dok pri **[prazno]** pada", „(**; ;** )" | 🔴 |
| 5 | „povećanje broja arena od 4 na 16 → sa ~277 na ~553" — spaja smoke-test (4 arene, 277) s bake-offom (12 → 16 arena, 495 → 553, +12 %) | 🔴 |
| 6 | Visjeće reference: **„§12 i §14"** (§3.4.6) i **„[pogl. X.Y]"** (§6.3.2) | 🔴 obje i dalje u tekstu |
| 7 | Naslovi tablica **ispod** tablice za Tablice 1–4 | 🟡 novije tablice ispravne, stare nisu |
| 8 | Referenca **[7]** (Baker) uz tvrdnju o ML-Agents Toolkitu — ondje pripada samo [8] | 🔴 |
| 9 | Naslovi 6.3.1 / 6.3.2 dvojezični („Sparse arm", „Shaped arm") | 🔴 |
| 10 | **Zaključak, Sažetak, Abstract, ključne riječi** prazni | 🟡 namjerno odgođeno |
| 11 | „Ostali prilozi i dokumentacija" prazno (nema Dodatka A) | 🔴 |
| 12 | Poglavlje 2 u prvom licu množine (8 mjesta) | 🔴 |
| 13 | Formule nenumerirane (~30) | 🔴 |
| 14 | Literatura u IEEE formatu; [1]–[4] bez autora | 🔴 |

---

## 5. Grafika — preostalo

Uneseno 27 slika. **Preostala 4 `TODO` markera** u tekstu, svi zahtijevaju izradu (ne postoje kao
datoteke):

| P | Mjesto | Grafika |
|---|---|---|
| 1 | §5.4 (odl. 562) | **Dijagram toka nagrade MA-POCA vs PPO** — grupni kanal → centralizirani kritičar nasuprot individualnom `AddReward`. Najveći omjer dojma i uloženog vremena; nosi cijelo objašnjenje zamke i veže se uz nedostajuće rezultate eksperimenta 2 |
| 1 | §4.2 (odl. 330) | Tortni dijagram wall-clock profila (env_step 78 %, od toga `communicator.exchange` 42 %; inference 14 %; gradijenti 6 %) |
| 1 | §4.4 (odl. 338) | Shematski tlocrt arene 20×20 s agentima, zidovima i raycast zrakama |
| 2 | §3.4.6 (odl. 291) | Graf potencijalne funkcije Φ(s) + stalnog člana (1−γ)·coef·(d/maxDist) koji **raste** s udaljenošću |
| 2 | §6.1 (odl. 579) | TensorBoard snimka `BaselineLoss` uz `ValueLoss` iz dim-testa (`TagTest_poca_01`) |

Neobavezno, ali podiže dojam: shema MA-POCA kritičara (§3.4.3) i 2×3 mreža sličica iz
Editor-inferencije koja vizualno suprotstavlja sparse Chasera (presijeca put) i shaped Chasera
(farmira izdaleka) u §6.3.2.

---

## 6. Preporučeni redoslijed rada

**Prvo (2–3 h, najveći učinak na dojam):**

1. **Prepisati Uvod** — izbaciti odlomke 115–120, popraviti „Error! Reference source not found.".
2. Popraviti N1–N6 (zvjezdice, naslov γ-tablice, redak stope hvatanja, zarezi, razmaci, tipfeleri).
3. `Ctrl+A` → `F9` da kazala odgovaraju stvarnom stanju.

**Drugo (pola dana):**

4. Prenijeti rezultate eksperimenta 2 (2×2 matrica + sonda isporuke) iz `Theory.md` §13.
5. Premjestiti brojke iz §5.6 u §6.4.3; u poglavlju 5 ostaviti samo dizajn i predikcije.
6. Popuniti prazne jednadžbe u §5.5 i ispraviti dvije visjeće reference (§12/§14, [pogl. X.Y]).
7. Ispraviti brojke o arenama (277 vs 495 → 553).

**Treće:**

8. Prenumerirati tablice po poglavljima; naslove Tablica 1–4 premjestiti iznad.
9. Numerirati formule; preurediti literaturu; poglavlje 2 u pasiv.
10. Izraditi grafike prioriteta 1 (dijagram toka nagrade, profil vremena, tlocrt arene).
11. **Na kraju:** Zaključak, Sažetak/Abstract, ključne riječi, Dodatak A.
