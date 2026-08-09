# Vodič za dovršetak diplomskog rada — konsolidirano stanje (9.8.2026.)

> **Kako nastaje ovaj dokument.** Ovo NIJE treća neovisna evaluacija. Ovo je spoj i ažuriranje
> `EVALUACIJA_diplomskog_rada.md` (rev. 2, 6.8.2026.) i `USKLADENOST_s_uputama_FESB.md`, provjeren
> **danas, izravnim čitanjem trenutnog `.docx`-a** (458 odlomaka — rad je aktivno mijenjan otkad su
> ta dva dokumenta napisana). Za svaku stavku iz ta dva dokumenta ovdje piše je li **✅ već
> riješeno**, **🔴 još otvoreno** ili **⚠️ promijenjeno stanje** (nešto se popravilo, ali ne posve
> onako kako je preporučeno, ili je otvoren novi problem). Claude ne dira `.docx` — sve što slijedi
> radi se ručno u Wordu.
>
> **Odluka o opsegu (potvrđeno s korisnikom, 9.8.2026.):** Faza B (nasumični rasporedi prepreka) je
> **izbačena iz plana rada** — ostaje samo kao spomen budućeg rada, nikad kao provedeno
> istraživanje. Cjeline koje sažimaju *cijeli* rad — **Zaključak, Sažetak/Ključne riječi,
> Title/Summary/Keywords, i ispravak broja poglavlja u Uvodu** — **namjerno nisu pisane u ovom
> vodiču** jer čekaju konačan oblik rada. Kad ta odluka bude do kraja "smirena", te dijelove treba
> pisati zasebno (uputa gdje i kako je u §7 ispod).

---

## 1. Prioritetna lista (izvedi ovim redom)

Legenda: 🔴 obavezno prije predaje · 🟡 jako preporučeno · ⚪ kozmetičko/ako ostane vremena.

### A — Sadržajne ispravke (jednorečenične, ali kritične)

| # | Mjesto u radu | Problem | Status | Akcija |
|---|---|---|---|---|
| A1 | §5.6 „Faza A", odl. 364–369 | Rezultati γ-sweepa opisani su **dvaput** — ovdje i u §6.4. | 🔴 još otvoreno | Poglavlje 5 smije sadržavati samo dizajn/predikcije. Ukloni prozu s brojkama iz 5.6 (odl. 365–369) i ostavi samo: opis metode (4 stupa, 9 konfiguracija, RQ-A/RQ-C) — brojke idu isključivo u §6. |
| A2 | Odl. 371 (uvod u pogl. 6) | Objašnjava razliku Faza A / Faza B kao da je Faza B stvarno planirana i uskoro dolazi | 🔴 još otvoreno, **sada gore nego prije** jer je Faza B u međuvremenu izbačena iz plana | Skrati na jednu rečenicu koja kaže da su korištene fiksne prepreke (Faza A) i da nasumični raspored ostaje ideja za budući rad — bez implikacije da je "sljedeći korak" ovog istraživanja. |
| A3 | Odl. 369, zadnja rečenica | „...što motivira Fazu B ... kao **sljedeći korak istraživanja**" | 🔴 još otvoreno | Ista ispravka kao A2 — preformulirati u smjeru "moguć pravac budućeg rada", ne aktivan plan. |
| A4 | Odl. 402 (§6.4.4) | „Faza B (nasumični rasporedi) **ispitat će** robustnost..." | 🔴 još otvoreno | Isto — promijeniti u kondicional / eksplicitno označiti kao neproveden budući rad. |
| A5 | Naslov pogl. 6 | „FAZA A" u naslovu | ✅ **riješeno** | Naslov sada glasi „Empirijski rezultati" bez oznake faze — potvrđeno u trenutnom tekstu. |
| A6 | §6.3.2 naslov | „proximity farming" nasuprot ispravka u tekstu na „shaping-farming" | ✅ **riješeno** | Naslov i tekst sada dosljedno koriste „shaping-farming". |
| A7 | §6.4.2, odl. 396 | „farming-zamka **(sparse)** Chasera" trebalo je biti (shaped) | ✅ **riješeno** | Tekst sada ispravno kaže „oblikovanog (shaped) Chasera". |
| A8 | §6.2 naslov | Trebao je postati „Validacijski rezultati (400k koraka)" | ✅ **riješeno** | Potvrđeno, naslov je točno taj. |
| A9 | Tablica 5 i 6 (§6.3) naslovi | Tablica 5 nije imala naslov | ✅ **riješeno** | Obje tablice sada imaju naslove: „Rezultati rijetke (sparse) ruke, 5M" i „Rezultati oblikovane (shaped) ruke, 5M". |
| A10 | Odl. 258 (§3.4.6) | Visjeća referenca „§12 i §14" — ne postoje takva poglavlja u ovoj strukturi | 🔴 još otvoreno | Zamijeni konkretnim brojem potpoglavlja čim T11/T12 dobiju svoje mjesto (vidi §3 dolje) — vjerojatno „§6.4.3" ili novo potpoglavlje s γ-sondama. |
| A11 | Odl. 389 (§6.3.2) | „...analiza slijedi u **[pogl. X.Y]**" — nepopunjen placeholder | 🔴 još otvoreno | Isto kao A10 — uputi na stvarno potpoglavlje kad tablica T11 (γ-sonde) dobije mjesto. |
| A12 | Odl. 272 (§3.5.2) | Parametri samoigre i dalje `⟨save_steps⟩` itd. u zagradama | 🔴 još otvoreno | Popuni iz stvarnog YAML-a (`TagMApoca.yaml` `self_play:` blok) — brojevi **ne smiju** biti izmišljeni, prepiši ih izravno iz konfiguracije. |
| A13 | Odl. 303 (§4.6) | „EXPLAIN: Konkretni parametri samoigre..." — bilješka urednika ostala u tekstu | 🔴 još otvoreno | Zamijeni stvarnom rečenicom kad A12 bude riješen — prirodno mjesto za iste brojeve (teorija u 3.5, parametrizacija u 4.6). |
| A14 | Odl. 362–363 (§5.5) | „pri γ=0,99taj horizont iznosi 100 odluka..., dok pri **[prazno]** pada..." i „u sve tri testirane konfiguracije **(; ; )**" | 🔴 još otvoreno | Popuni: drugi γ je **0,8** (5 odluka / 25 fizikalnih koraka); tri konfiguracije u zagradi su **γ=0,8; γ=0,9; γ=0,99** (sonde iz `Theory.md` §14 „probe results"). |
| A15 | Odl. 367 (§5.6) | „stopa hvatanja rasla je s 0,86 pri γ=0,8 do **1,00 pri γ=0,99**" | 🔴 **brojčana netočnost** | Po `Theory.md` §14 (tablica 9 pokreta) vrhunac 1,00 je **pri γ=0,95** (seed-mean); γ=0,99 seed-mean je **0,99**. γ=0,99 se brani brzinom učenja i stabilnošću, ne najvišom stopom. Ispravi rečenicu. |
| A16 | Odl. 304 (§4.6) | „Povećanje broja arena od **4 na 16** → sa ~277 na ~553 (+100%)" | 🔴 **spaja dva različita mjerenja** | Po `Theory.md` §10: 277 je iz **4-arenskog smoke-testa**; bake-off je zaseban par mjerenja **12 arena = 495** i **16 arena = 553** (+12% u odnosu na 12, ne na 4). Ispravi na: "smoke-test s 4 arene: ~277 koraka/s; bake-off 12→16 arena: 495→553 koraka/s (+12%)". |
| A17 | Odl. 304, isto mjesto | Nedosljednost 78% (§4.2, odl. 290) vs „~80%" (§4.6.1, odl. 309) | 🟡 jako preporučeno | Uskladi na jednu vrijednost — 78% je preciznija (izravno iz `timers.json`). |
| A18 | Odl. 115 (Uvod) | „Rad je podijeljen na **sedam** poglavlja" | 🔴 još otvoreno, **ali odgođeno** (vidi §7) | Ne popravljati broj dok se ne zna konačna struktura (ovisi hoće li se dodati Poglavlje 7 „Budući rad" — vidi A2–A4). |

### B — Nedostajuće tablice (podaci su gotovi, prijenos je brz)

| # | Tablica | Gdje ide | Status | Izvor podataka |
|---|---|---|---|---|
| T-val | 400k validacija: sparse vs shaped (ELO gap, catch rate, ep. length, Group Reward) | §6.2, uz postojeći TODO (odl. 379) | 🔴 nedostaje | `Theory.md` §11, tablica "Headline numbers (final-window values)" — 4 retka, brojevi navedeni u §2 dolje. |
| T-ppo | Matrica 2×2 (POCA/PPO × sparse/shaped): catch rate, ELO | Nova, u §6 (predlažem novo 6.5, poslije 6.4) | 🔴 nedostaje — **§5.4 ima dizajn eksperimenta, ali NIGDJE u radu nema njegovih rezultata** | `Theory.md` §13 „Results — the 2×2 at 5M". |
| T-probe | Sonda kanalske isporuke (grupni / grupni+individualni / individualni) | Isto novo 6.5 | 🔴 nedostaje | `Theory.md` §13 „Follow-up result — a partial rescue". |
| T-g-probe | γ-sonde RQ-B (0,8 / 0,9 / 0,99) + omjer žetve ≈1:8:19 | Isto novo 6.5 ili 6.6 | 🔴 nedostaje — ovime se popunjava i A10/A11/A14 | `Theory.md` §14 „Probe results — RQ-B". |
| **T12** | **Faza A — svih 9 pokreta** (γ, sjeme, catch, ELO Chaser/Runner, gap, ep. length, TimeToCatch, GroupR) | Novo 6.6 (ili gdje god §5.6/§6.4 rezultati završe konsolidirani, vidi A1) | 🔴 **najhitnije nedostaje** — trenutno postoji cijela rasprava (§6.4.3, §5.6) o γ-krivulji bez ijednog retka podataka koji je potkrepljuje | `Theory.md` §14, tablica "Phase A results" — **9 redaka, gotovi brojevi, kopiraj izravno** (vidi §2 dolje za cijelu tablicu spremnu za lijepljenje). |

### C — Formatske ispravke iz `USKLADENOST_s_uputama_FESB.md` (provjereno, sve još otvoreno)

Ovaj dio nije ponovno provjeravan liniju-po-liniju danas (formatski detalji poput fonta veličine
naslova ne mijenjaju se slučajno), ali brzom provjerom TOC-a i stilova u trenutnom `.docx`-u ništa
od ovog nije riješeno:

- 🔴 Naslovi tablica su ispod tablica → moraju biti **iznad**.
- 🔴 Numeracija tablica/slika je linearna (`Tablica 1…6`, `Slika 21…41`) → treba biti **po
  poglavljima s točkom** (`Tablica 6.1.`, `Slika 3.2.` — trenutni brojevi slika poput „21", „31"
  čak izgledaju kao greška jer nedostaje razdjelna točka).
- 🔴 Nijedna od 30+ jednadžbi nije numerirana.
- 🔴 Literatura je u IEEE stilu (inicijal-prezime), Upute traže Prezime-inicijal format; jedinice
  [1]–[4] nemaju autora.
- 🔴 Poglavlje 2 pisano u prvom licu množine ("Recimo da...", "Definirajmo...") — Upute traže
  neodređeno/treće lice ili pasiv. 8 mjesta, sva u poglavlju 2 (§2.1–2.2).
- 🔴 Popis oznaka i kratica prazan (vidi §4 dolje — potpuna izrada spremna za lijepljenje).
- 🔴 Kazalo kodova prazno jer u radu nema nijednog isječka koda.
- 🔴 `Heading 2` stil je 13pt umjesto propisanih 12pt; koristi se i četvrta razina naslova
  (`Heading4`) unatoč preporuci od najviše tri razine.
- Puni popis s prijedlozima teksta ostaje u `USKLADENOST_s_uputama_FESB.md` §3–§7 — ovaj vodič ga
  ne duplicira, samo potvrđuje da je još uvijek na snazi.

---

## 2. Tablice spremne za copy-paste

### T12 — Faza A: svih 9 pokreta (najhitnije, ide u novo 6.6 ili gdje god se γ-sweep rezultati konsolidiraju)

*Izvor: `docs/Theory.md`, odjeljak "Phase A results — sparse gamma sweep, 4 FIXED pillars",
srednje vrijednosti zadnjih 5 TensorBoard točaka (~zadnjih 250k koraka).*

| γ (sjeme) | Stopa hvatanja | ELO Chaser | ELO Runner | ELO razlika | Duljina epizode | TimeToCatch (fiz. koraci) | Group Reward (Chaser) |
|---|---|---|---|---|---|---|---|
| 0,8 (s1) | 0,92 | 1688 | 738 | 950 | 157 | 293 | +1,01 |
| 0,8 (s2) | 0,93 | 1729 | 729 | 1001 | 161 | 318 | +1,07 |
| 0,8 (s3) | 0,74 | 1657 | 770 | 886 | 259 | 407 | +0,39 |
| 0,9 (s1) | 0,96 | 1780 | 750 | 1030 | 101 | 187 | +1,22 |
| 0,95 (s1) | **1,00** | 1875 | 664 | 1211 | 53 | 135 | +1,43 |
| 0,99 (s1) | 0,99 | 1909 | 660 | **1249** | **50** | **118** | +1,42 |
| 0,995 (s1) | 0,89 | 1448 | 1053 | 395 | 217 | 448 | +1,07 |
| 0,995 (s2) | **1,00** | 1933 | 681 | 1253 | 45 | 114 | +1,44 |
| 0,995 (s3) | **1,00** | 1946 | 689 | 1257 | 43 | 105 | +1,43 |

Seed-mean po γ (za graf osjetljivosti, Slika/Fig. 10): catch **0,86 → 0,96 → 1,00 → 0,99 → 0,96**
za γ = 0,8 → 0,9 → 0,95 → 0,99 → 0,995; ELO razlika **946 → 1030 → 1211 → 1249 → 968**.

### T-val — Validacija 400k: sparse vs shaped

*Izvor: `Theory.md` §11 "Headline numbers".*

| Mjera | Sparse (bez oblikovanja) | Shaped (PBS coef 0,5) | Čitanje |
|---|---|---|---|
| ELO razlika (Chaser−Runner) | +21,9 | **+72,7** | oblikovanje ≈ 3× veća kompetitivna razdvojenost |
| Environment/Catch (stopa hvatanja) | ~0,08 | **~0,21** | oblikovanje ≈ 2,5–3× stopa hvatanja |
| Environment/Episode Length | 386 | **374** | oblikovani Chaser hvata nešto brže |
| Group Cumulative Reward (Chaser) | −0,91 | **−0,75** | stvarni ishod igre (neovisan o oblikovanju) — oblikovani Chaser gubi manje |

### T-ppo — Matrica 2×2 (POCA/PPO × sparse/shaped, 5M) + sonda isporuke

*Izvor: `Theory.md` §13. Napomena: budući da §5.4 u radu već postoji kao dizajn eksperimenta,
ova tablica ide u rezultate (§6), ne u §5 — vidi pravilo iz A1.*

| Konfiguracija | Stopa hvatanja | ELO Chaser | Napomena |
|---|---|---|---|
| POCA + sparse | ~1,00 | visok | referentna točka |
| PPO + sparse | ~0,90 | visok | PPO malo slabiji od POCA |
| PPO + shaped | **~0,98** | visok | PPO **izbjegava** zamku |
| POCA + shaped | **~0,01** | nizak | **jedina ćelija koja upada u farming-zamku** |

Sonda kanalske isporuke (dodatna dijagnostika, ista tablica ili posebna): samo-grupni kanal ≈ 0,01
stopa hvatanja; grupni+individualni terminal ≈ 0,12 (djelomičan oporavak, još raste); samo-
individualni (PPO-stil) ≈ 0,98. Zaključak (već je u tekstu §6.4.2, sada dobiva broj koji potkrepljuje
tvrdnju): zamka ima **dva uzroka** — kanal isporuke nagrade (nužan uvjet) i MA-POCA-ova algoritamska
osjetljivost (dovoljan uvjet), vidi odl. 396–397.

---

## 3. Popis mjesta gdje treba grafika (osvježeno, uz jednu bitnu ispravku)

**Bitna ispravka u odnosu na `EVALUACIJA_diplomskog_rada.md`:** ta evaluacija je označila "glavnu
sliku rada" (sparse-vs-shaped kroz 5M, 3 seeda, ELO + Group Reward + individualna nagrada) kao 📁
"već izrađeno, samo umetni". **To više ne stoji.** `Theory.md` §12 eksplicitno kaže da je ta
agregacija (mean ± std kroz 3 sjemena, error bands) *"pending"* — skripta za agregaciju sjemena
preko TensorBoard event-fileova ne postoji (`experiments/analysis/` sadrži samo `parse_tb.py` i
`plot_gamma.py`, oba specifična za druge pokuse). **Ova slika treba novi analitički rad**, ne samo
umetanje — vrijedi zasebna sesija koja piše skriptu po uzoru na `plot_gamma.py`, prije nego što se
može umetnuti bilo što u §6.3.

Status: ✅ postoji i može se umetnuti odmah · 🔨 treba izraditi (dijagram/screenshot) · 🛠️ treba
**novi analitički kod** prije nego što uopće postoji što umetnuti.

| P | Mjesto (TODO odlomak) | Grafika | Status |
|---|---|---|---|
| 1 | §3.4.6, odl. 252 | Graf potencijalne funkcije Φ(s) po udaljenosti + graf stalnog člana (1−γ)·coef·(d/maxDist) koji raste s udaljenošću | 🔨 trivijalno u matplotlibu, formula je poznata (§3.4.6 samog rada) |
| 1 | §4.2, odl. 291 | Tortni dijagram wall-clock profila: env_step ~78% (od toga communicator.exchange 42%), inference ~14%, gradijenti ~6% | 🔨 iz `timers.json`, brojevi već poznati |
| 1 | §4.4, odl. 299 | Shematski tlocrt arene 20×20 s agentima, zidovima, primjerom raycast zraka | 🔨 vlastiti dijagram |
| 1 | §4.6, odl. 307 | Screenshot Unity Editora, `Scene_V2`, 16 arena na X-osi | 🔨 screenshot (mora se raditi u Unityju, ne generira se kodom) |
| 1 | §5.4, odl. 360 | Dijagram toka nagrade MA-POCA vs PPO (grupni kanal→centralizirani kritičar nasuprot `Agent.AddReward`) | 🔨 vlastiti dijagram — nosi cijelo objašnjenje zamke, visoka vrijednost za trud uložen |
| 1 | §6.1, odl. 376 | TensorBoard screenshot: BaselineLoss uz ValueLoss za oba agenta (dim-test) | 🔨 screenshot iz arhiviranog dim-test runa (`TagTest_poca_01`), ako TB podaci još postoje na disku |
| 1 | §6.2, odl. 379 | TensorBoard grafovi: ELO divergencija + catch rate/ep. length za 400k validaciju | ✅ **postoji** — `docs/figures/validation/tb_elo.png` i `tb_catch_episodelen.png` (opcijski i `tb_overview.png`, `tb_policy.png`) |
| 1 | novo 6.5 (PPO 2×2) | Fig. 5/6 — stopa hvatanja i ELO za sve 4 ćelije matrice | ✅ **postoji** — `docs/figures/ppo/tb_2x2_catch.png`, `tb_2x2_elo.png` |
| 1 | novo 6.5 (sonda isporuke) | Fig. 7 — 3 krivulje stope hvatanja po kanalu isporuke | ✅ **postoji** — `docs/figures/ppo/tb_probe_delivery.png` |
| 1 | novo 6.6 (γ-sonde) | Fig. 8 — catch pod 1% za sve γ + ljestve žetve individualne nagrade | ✅ **postoji** — `docs/figures/gamma/tb_probe_gamma.png` |
| **1** | **novo 6.6 (γ-sweep)** | Fig. 9 — krivulje učenja po γ, min–max pojas; Fig. 10 — krivulja osjetljivosti; Fig. 11 — ELO svih 9 pokreta | ✅ **postoji sve troje** — `docs/figures/gamma/{sweepA_catch_curves,sweepA_sensitivity,tb_sweepA_elo}.png` (+ `tb_sweepA_overview.png` kao dodatni raw pregled) |
| **1** | **§6.3 (glavni rezultati, 5M)** | ELO + Group Reward + individualna nagrada, sparse vs shaped, 3 sjemena, min-max pojas | 🛠️ **treba se najprije izraditi analitička skripta** (vidi napomenu gore) — nije samo umetanje |
| 2 | §3.4.3, uz „self-attention" | Shema arhitekture MA-POCA kritičara (koderi → RSA blok → vrijednosna glava + kontrafaktična baza) | 🔨 vlastiti dijagram |
| 2 | novo 3.5 (samoigra, ako se dodatno ilustrira) | Dijagram petlje samoigre: trenirajući tim ↔ snapshot protivnik | 🔨 vlastiti dijagram (opcionalno — tekst 3.5 je već potpun i jasan bez slike) |
| 2 | §6.3.2, uz opis farmiranja | 2×3 mreža sličica iz Editor-inferencije: sparse Chaser presijeca put vs shaped Chaser loiteri | 🔨 snimka zaslona iz Unity Editor-inference sesije |
| 3 | kraj §6 | Sumarna vizualna tablica: RQ-A/B/C → predikcija → ishod (potvrđeno/odbačeno) | 🔨 jednostavna tablica/grafika, podaci već postoje u nalazima §6.4 |

**Ukupno trenutno stanje: 7 slika spremno za umetanje odmah (7 od 11 numeriranih TensorBoard
figura iz `Theory.md`), 1 zahtijeva nov analitički kod prije bilo čega, ~8 treba izraditi kao
dijagram/screenshot.**

---

## 4. Popis oznaka i kratica — spreman za lijepljenje

*(Abecedni red, kako Upute traže. Sastavljeno izravno iz kratica koje se stvarno pojavljuju u
tekstu — nijedna nije izmišljena.)*

| Kratica | Značenje |
|---|---|
| AI | umjetna inteligencija (engl. *Artificial Intelligence*) |
| API | programsko sučelje (engl. *Application Programming Interface*) |
| COMA | engl. *Counterfactual Multi-Agent Policy Gradients* — MARL algoritam na kojem se temelji MA-POCA |
| CPU | centralna procesorska jedinica (engl. *Central Processing Unit*) |
| CTDE | centralizirano treniranje, decentralizirano izvođenje (engl. *Centralized Training, Decentralized Execution*) |
| ELO | sustav ocjenjivanja relativne uspješnosti (izvorno za šah, autor Arpad Elo) |
| GPU | grafička procesorska jedinica (engl. *Graphics Processing Unit*) |
| gRPC | protokol za međuprocesnu komunikaciju korišten između Unity i Python procesa |
| IPC | međuprocesna komunikacija (engl. *Inter-Process Communication*) |
| MA-POCA | engl. *Multi-Agent POsthumous Credit Assignment* — algoritam korišten za treniranje agenata |
| MARL | višeagentno učenje pojačanjem (engl. *Multi-Agent Reinforcement Learning*) |
| MDP | Markovljev proces odlučivanja (engl. *Markov Decision Process*) |
| ML-Agents | Unity Machine Learning Agents Toolkit — korišteni simulacijski i trenirajući okvir |
| ONNX | engl. *Open Neural Network Exchange* — format u kojem se izvoze istrenirani modeli |
| PBS | oblikovanje nagrade zasnovano na potencijalu (engl. *Potential-Based Shaping*) |
| POMDP | djelomično promatrljiv Markovljev proces odlučivanja (engl. *Partially Observable MDP*) |
| PPO | proksimalna optimizacija politike (engl. *Proximal Policy Optimization*) |
| RML | pojačajno (podržano) strojno učenje (engl. *Reinforced/Reinforcement Machine Learning*) |
| RQ | istraživačko pitanje (engl. *Research Question*) |
| RSA | blok mehanizma samopažnje unutar MA-POCA kritičara (engl. *residual self-attention*) |
| SAC | engl. *Soft Actor-Critic* — algoritam podržanog učenja spomenut uz PPO |
| SDK | razvojni komplet alata (engl. *Software Development Kit*) |
| TD(λ) | temporalna razlika s parametrom traga λ (engl. *Temporal Difference*) |
| TensorBoard | alat za vizualizaciju metrika tijekom treniranja |
| TRPO | optimizacija politike temeljena na regiji povjerenja (engl. *Trust Region Policy Optimization*) |

*(23 stavke. Ako se pri finalnom čitanju pojavi još pokoja kratica u novom tekstu, dodaj je na
odgovarajuće mjesto u abecedi.)*

---

## 5. Struktura poglavlja — trenutno stanje (za orijentaciju)

Cijeli trenutni sadržaj (numeracija odgovara `.docx` TOC-u, 9.8.2026.):

```
1 UVOD                                                    [garbled odl. 115–120, vidi §7]
2 TEORIJSKA PODLOGA                                       [potpuno, stil treba pasiv — §1.C]
   2.1 Osnove podržanog strojnog učenja
   2.2 Koncepti strojnog učenja (2.2.1–2.2.4)
   2.3 Markovljev proces odlučivanja (2.3.1)
3 Pregled korištenih rml Algoritama                       [potpuno]
   3.1 PPO (3.1.1)   3.2 Poveznica PPO/MARL/MA-POCA   3.3 MARL (3.3.1)
   3.4 MA-POCA (3.4.1–3.4.6, uklj. TODO Φ(s) slika)
   3.5 Samoigra i ELO (3.5.1–3.5.3)                       [✅ NOVO, potpuno, iz NACRT-a]
4 Okruženje i implementacija                               [potpuno, 3 TODO slike + A12/A13 rupe]
   4.1–4.6 (4.6.1 Bottleneck)
5 Eksperimentalni dizajn
   5.1 RQ-A/B/C   5.2 Mjerne veličine (5.2.1–5.2.4)        [✅ NOVO, potpuno, iz NACRT-a]
   5.3 Eksperiment 1 (sparse/shaped)   5.4 Eksperiment 2 (POCA/PPO, TODO dijagram)
   5.5 Eksperiment 3 (γ sweep, prazne jednadžbe — A14)
   5.6 Faza A (9 konfiguracija — SADRŽI REZULTATE, treba preseliti — A1)
6 Empirijski rezultati                                     [naslov ✅ ispravljen — A5]
   6.1 BaselineLoss (TODO screenshot)   6.2 Validacija 400k (TODO grafovi, nedostaje T-val)
   6.3 Glavni rezultati 5M (nedostaje glavna slika — treba analitički kod, §3)
       6.3.1 Sparse   6.3.2 Shaped/farming (A10/A11 visjeće reference)
   6.4 Rasprava — FAZA A (A2/A3/A4 Phase B framing)
       6.4.1–6.4.4
   [NEDOSTAJE: rezultati Eksperimenta 2 (PPO 2×2) — nigdje u radu, samo dizajn u §5.4]
   [NEDOSTAJE: T12 tablica 9 pokreta — rasprava postoji, brojevi ne]
ZAKLJUČAK                                                   [🔴 prazno — ODGOĐENO, §7]
LITERATURA                                                  [potpuno, 15 jedinica, format treba IEEE→FESB]
Kazalo slika, tablica i kodova                              [Kazalo kodova prazno — nema isječaka koda]
Popis oznaka i kratica                                      [🔴 prazno → §4 ovog vodiča]
Ostali prilozi i dokumentacija                              [🔴 prazno — treba Dodatak A s isječcima koda]
SAŽETAK/ABSTRACT I KLJUČNE RIJEČI/KEYWORDS                  [🔴 sve prazno — ODGOĐENO, §7]
```

---

## 6. Isječci koda za „Ostali prilozi i dokumentacija" (4–6 kratkih listinga)

USKLADENOST §3 traži dokumentaciju programske podrške — rad trenutno nema nijedan isječak koda.
Predloženi minimalni set (svi već postoje u repozitoriju, samo ih treba prekopirati kao `Listing`):

1. Potencijalna funkcija Φ(s) i član oblikovanja F — `Assets/Scripts/Reward/TagReward.cs`
2. `AddGroupReward` + `EndGroupEpisode` poziv pri hvatanju — `Assets/Scripts/TagArenaManager.cs`
3. YAML `poca` trener + `self_play` blok — `TagMApoca.yaml` (ista mjesta koja popunjavaju A12)
4. `StatsRecorder` poziv za `Environment/Catch` / `Environment/TimeToCatch` — `TagArenaManager.cs`

Uz to, Dodatak A: poveznica na repozitorij (`Private-Endgame`) i kratak popis priloženih datoteka
(konfiguracije, `Theory.md`, istrenirani `.onnx` modeli u `Assets/Models/5M/`).

---

## 7. Odgođeno — piši tek kad Faza B odluka bude konačna

Ova tri/četiri dijela **namjerno nemaju nacrt teksta** u ovom vodiču:

- **Zaključak** (do 2 str.) — sažima RQ-A/B/C ishode, mora spomenuti i konačan status Faze B.
- **Sažetak** (do 1 str.) + **Ključne riječi** (3–5)
- **Title / Summary / Keywords** (engleski)
- **Ispravak "sedam poglavlja" u Uvodu** (odl. 115) — čeka konačan broj poglavlja (hoće li biti
  dodano zasebno poglavlje 7 „Budući rad"? vidi preporuku niže)

Kad odluka o Fazi B bude do kraja zatvorena (ili barem framing u A2–A4 iznad bude ispravljen), ova
četiri dijela idu **prva na redu** u sljedećoj sesiji — sav potreban materijal (nalazi, brojke,
citati) već postoji u `Theory.md` §§14–15 i u poglavlju 6 samog rada, pisanje je uglavnom sažimanje
već postojećeg teksta, ne novo istraživanje.

**Preporuka za strukturu budućeg rada** (ne piši sada, samo za orijentaciju): EVALUACIJA je
predložila zasebno poglavlje 7 „Budući rad" prije Zaključka, koje bi objedinilo A2–A4 ispravke (Faza
B, timska ekspanzija 2v1/2v2, sweep gustoće prepreka, prag shaping-koeficijenta, seed-hardening,
kvalitativna taksonomija — sve već pobrojano u `Theory.md` §15). To ostaje razuman prijedlog, ali
odluka (dodati li ga ili sažeti u par rečenica unutar Zaključka) čeka finalni opseg rada.
