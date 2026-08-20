# Nacrt poglavlja za Fazu B — za ručno umetanje u `.docx`

> **Što je ovo.** Gotov tekst na hrvatskome, pisan u stilu postojećega rada, za dva nova
> potpoglavlja (dizajn u 5. poglavlju, rezultati u 6. poglavlju) te dvije ispravke postojećih
> odlomaka koji Fazu B još opisuju kao neprovedenu.
>
> **Numeracija se nastavlja na postojeću:** zadnja slika u radu je `Slika 6.23`, zadnja tablica
> `Tablica 6.6`. Novi prilozi stoga počinju od **`Slika 6.24`** i **`Tablica 6.7`**.
>
> **Mjesta za slike označena su blokovima `[MJESTO ZA SLIKU …]`** — svaki sadrži točan naziv
> TensorBoard grafa koji treba snimiti i popis pokretanja (engl. „runs") koje treba uključiti u
> prikaz. Snimke se rade u prikazu „Time Series", uz zaglađivanje (engl. „smoothing") 0,8, svaka
> kartica pojedinačno uvećana — isti postupak kao za `Slike 6.9`–`6.20`.
>
> **Podatci u tablicama su izmjereni**, srednje vrijednosti zadnjih pet TensorBoard točaka
> (posljednjih ~250 000 koraka), izvučene izravno iz `tfevents` datoteka.

---

## A. Ispravke postojećega teksta (dva odlomka)

### A1. Potpoglavlje 5.6, odlomak koji završava riječima „…planirano kao neposredan nastavak istraživanja"

**Postojeći tekst** završava rečenicom: *„Faza B nije provedena u okviru ovog rada — programska
podrška za nasumično raspoređivanje prepreka je implementirana i ispitana, a njezino je izvođenje
planirano kao neposredan nastavak istraživanja (v. poglavlje o budućem radu)"*.

**Zamijeniti s:**

> Upravo je uklanjanje te dvojbe svrha Faze B: nasumičnim mijenjanjem rasporeda u svakoj epizodi
> pamćenje pojedinačne geometrije prestaje biti korisno, pa se može provjeriti donosi li
> nepredvidljiv raspored prepreka konačno stratešku prednost bjeguncu. Faza B provedena je i njezin
> je dizajn opisan u potpoglavlju 5.7, a rezultati u potpoglavlju 6.6.

### A2. Potpoglavlje 6.5.4 („Prepreke i intuicija o prednosti bjegunca"), zadnja rečenica

**Postojeći tekst** završava: *„Faza B (nasumični rasporedi) ispitat će robustnost: je li naučena
strategija Chasera memorirala fiksnu geometriju ili je zaista naučila generalnu navigaciju."*

**Zamijeniti s:**

> Faza B (nasumični rasporedi) ispitala je robustnost toga nalaza i potvrdila drugu mogućnost:
> strategija lovca nije bila vezana uz zapamćenu geometriju, nego je riječ o općenitoj navigaciji.
> Nasumično raspoređivanje prepreka u svakoj epizodi nije proizvelo nikakvu mjerljivu promjenu
> (potpoglavlje 6.6), čime negativan rezultat za RQ-C prestaje biti uvjetan.

---

## B. Novo potpoglavlje 5.7 — dizajn Faze B

### 5.7 Faza B: nasumične prepreke

Faza B izvodi isti pokus kao Faza A, ali uz jednu izmjenu: na početku svake epizode četiri se stupa
raspoređuju nasumično unutar arene, uz zadržavanje najmanje dopuštene međusobne udaljenosti i
odmaka od zidova. Geometrija arene time se mijenja iz epizode u epizodu, pa strategija koja se
oslanja na zapamćeni raspored prestaje biti upotrebljiva. Nasumični je raspored upravljan
parametrom okoline `obstacle_layout: 1`, dok je broj prepreka i dalje `num_obstacles: 4`.

Za razliku od Faze A, koja je obuhvatila devet konfiguracija kroz pet vrijednosti diskontnog
faktora, Faza B provedena je **isključivo pri γ = 0,99, kroz tri sjemena** (engl. „seeds"). Razlog
je metodološki i praktičan. Faza A već je utvrdila γ = 0,99 kao radnu točku: pri toj vrijednosti
lovac postiže vršne rezultate na platou krivulje osjetljivosti, uči najbrže, a izostaje
nestabilnost zamijećena pri γ = 0,995. Ponavljanje cjelovitoga pretraživanja po γ pri nasumičnim
rasporedima trošilo bi devet punih pokretanja na pitanje na koje je već odgovoreno, dok se pitanje
zbog kojega Faza B postoji — mijenja li nepredvidljiv raspored prepreka ravnotežu između lovca i
bjegunca — u cijelosti razrješava mjerenjem u toj jednoj radnoj točki. Tri sjemena osiguravaju
raspon vrijednosti umjesto pojedinačnoga mjerenja.

Cijena je toga izbora **interakcija između γ i rasporeda prepreka**, koja u ovom radu ostaje
neizmjerena: nije provjereno zadržava li krivulja osjetljivosti na γ isti oblik kada je pamćenje
geometrije onemogućeno. Riječ je o svjesno prihvaćenom ograničenju opsega, a ne o propustu;
navedeno je među smjerovima budućega rada.

Pre-registrirana predikcija za RQ-C, zapisana prije pokretanja svih pokusa s preprekama, glasila
je: *nasumični rasporedi uče sporije i završavaju na nižoj razini od fiksnih pri istoj vrijednosti
γ*. Uz nju je unaprijed definiran i uvjet potvrde: svaki osjetan pad stope hvatanja ili sužavanje
ELO razlike u odnosu na fiksni raspored potvrdio bi da nepredvidljiva geometrija donosi prednost
bjeguncu.

Prije pokretanja Faze B izvršena je i provjera ispravnosti izvedbene datoteke. Ispravak generatora
slučajnih brojeva, kojim je raspored prepreka vezan uz `--seed` parametar trenera, unesen je nakon
dovršetka Faze A, pa je izvedbena datoteka ponovno izgrađena i podvrgnuta kratkom ispitnom
pokretanju od 50 000 koraka. Dva uzastopna ispitna pokretanja s istim sjemenom dala su **identične
vrijednosti svih mjernih veličina do četiri decimale**, čime je potvrđeno da su rasporedi prepreka
u Fazi B doista ponovljivi.

Rezultati Faze B izneseni su u potpoglavlju 6.6.

---

## C. Novo potpoglavlje 6.6 — rezultati Faze B

### 6.6 Rasprava rezultata – FAZA B

Sva tri pokretanja (`POCA_sparse_obsR_g099_s1`, `s2` i `s3`) dovršila su 5 · 10⁶ koraka po
ponašanju i izvezla obje mreže u `.onnx` obliku. Zapis izvedbene datoteke u svim trima
pokretanjima sadrži redak `[ObstacleManager] num_obstacles=4, layout=random`, čime je potvrđeno da
je nasumični raspored doista bio aktivan, kao i redak
`[TagAgent] distance_shaping_coef=0,00`, koji potvrđuje rijetku (engl. „sparse") nagradu. Nije
zabilježena nijedna Unity pogreška ni ijedna nekonačna vrijednost među svim praćenim mjernim
veličinama.

**Tablica 6.7** Rezultati Faze B — tri sjemena pri γ = 0,99, nasumični raspored prepreka

| γ = 0,99 (sjeme) | Stopa hvatanja | ELO Chaser | ELO Runner | ELO razlika | Duljina epizode | TimeToCatch (fiz. koraci) | Group Reward (Chaser) |
|---|---|---|---|---|---|---|---|
| 0,99 (s1) | 0,999 | 1958 | 680 | 1277 | 46,7 | 117 | +1,437 |
| 0,99 (s2) | 0,999 | 1850 | 607 | 1243 | 45,2 | 114 | +1,440 |
| 0,99 (s3) | 1,000 | 1945 | 695 | 1250 | 49,2 | 125 | +1,436 |
| **Prosjek** | **0,999** | **1918** | **661** | **1257** | **47,1** | **119** | **+1,438** |

#### 6.6.1 Usporedba s fiksnim rasporedom i otvorenom arenom

Kako bi se izmjerio učinak nasumičnoga rasporeda, rezultati Faze B uspoređeni su s dvjema
referentnim skupinama: s pokretanjima Faze A pri γ ≥ 0,95, koja dijele plato krivulje
osjetljivosti, te s pokretanjima u otvorenoj areni bez prepreka iz potpoglavlja 6.4. Stupac
„do 0,95" označava broj koraka potreban da klizni prosjek stope hvatanja dosegne vrijednost 0,95,
odnosno brzinu učenja.

**Tablica 6.8** Usporedba triju uvjeta — otvorena arena, fiksne prepreke i nasumične prepreke

| Uvjet | Broj pokretanja | Stopa hvatanja (raspon) | ELO razlika (raspon) | Duljina epizode | do 0,95 |
|---|---|---|---|---|---|
| Otvorena arena, bez prepreka | 3 | 0,994 (0,018) | 1218 (54) | 53,7 | 1,23 · 10⁶ |
| Fiksne prepreke, γ ≥ 0,95 | 4 | 0,998 (0,005) | 1242 (46) | 47,9 | 1,12 · 10⁶ |
| **Nasumične prepreke, γ = 0,99** | 3 | **0,999 (0,001)** | **1257 (34)** | **47,1** | **0,97 · 10⁶** |

Jedina strogo istovrsna usporedba jest ona pri istoj vrijednosti γ, između Faze B i pokretanja
`POCA_sparse_obsF_g099_s1`: stopa hvatanja 0,999 naspram 0,995, ELO razlika 1257 naspram 1249,
duljina epizode 47,1 naspram 50,0, TimeToCatch 118,8 naspram 118,3 te 0,97 · 10⁶ naspram
0,90 · 10⁶ koraka do stope hvatanja 0,95. **Sve razlike leže unutar raspona među sjemenima.**

[MJESTO ZA SLIKU — TensorBoard, `Environment/Catch`]
*Pokretanja: `POCA_sparse_obsR_g099_s1`, `s2`, `s3`. Prikaz „Time Series", zaglađivanje 0,8.*

**Slika 6.24** Stopa hvatanja (`Environment/Catch`) za tri sjemena Faze B, 5 · 10⁶ koraka

[MJESTO ZA SLIKU — TensorBoard, `Environment/Group Cumulative Reward`]
*Ista tri pokretanja; prikazati ponašanja Chaser i Runner.*

**Slika 6.25** Grupna kumulativna nagrada (`Environment/Group Cumulative Reward`), Faza B,
5 · 10⁶ koraka

[MJESTO ZA SLIKU — TensorBoard, `Environment/Episode Length`]

**Slika 6.26** Duljina epizode (`Environment/Episode Length`), Faza B, 5 · 10⁶ koraka

[MJESTO ZA SLIKU — TensorBoard, `Self-play/ELO`]
*Prikazati obje krivulje (Chaser i Runner) za sva tri sjemena.*

**Slika 6.27** ELO ocjena u samoigri (`Self-play/ELO`), Faza B, 5 · 10⁶ koraka

[MJESTO ZA SLIKU — TensorBoard, `Policy/Entropy`]

**Slika 6.28** Entropija politike (`Policy/Entropy`), Faza B, 5 · 10⁶ koraka

[MJESTO ZA SLIKU — TensorBoard, `Environment/Catch`, usporedni prikaz]
*Ključna slika ovoga potpoglavlja. Na istim osima prikazati tri pokretanja Faze B
(`POCA_sparse_obsR_g099_s*`) i pokretanje Faze A `POCA_sparse_obsF_g099_s1`, kako bi se vidjelo
preklapanje krivulja.*

**Slika 6.29** Usporedba stope hvatanja pri fiksnom i nasumičnom rasporedu prepreka, γ = 0,99

#### 6.6.2 Nalazi u odnosu na pre-registriranu predikciju

Pre-registrirana predikcija za RQ-C **opovrgnuta je u oba svoja dijela**.

**Prvo, tvrdnja da nasumični rasporedi završavaju na nižoj razini nije se potvrdila.** Sve konačne
mjerne veličine podudaraju se s onima pri fiksnom rasporedu ili ih neznatno nadmašuju, a sve
razlike leže unutar raspona među sjemenima. Bjegunac nije stekao nikakvu prednost: njegova ELO
ocjena (661) i grupna nagrada lovca (+1,438) praktički su jednake vrijednostima pri fiksnom
rasporedu (660 odnosno +1,422).

**Drugo, tvrdnja da nasumični rasporedi uče sporije također se nije potvrdila.** Za dosezanje stope
hvatanja 0,95 pri nasumičnom je rasporedu bilo potrebno 0,97 · 10⁶ koraka, naspram 0,90 · 10⁶ pri
fiksnom rasporedu i istoj vrijednosti γ — razlika koja je manja od raspona među samim sjemenima
Faze B (0,80 · 10⁶ do 1,20 · 10⁶). U odnosu na objedinjenu skupinu fiksnih pokretanja nasumični je
raspored nominalno čak i brži (0,97 · 10⁶ naspram 1,12 · 10⁶), no ta skupina objedinjuje tri
različite vrijednosti γ, a upravo γ upravlja brzinom učenja, pa se ta razlika ne smije tumačiti kao
ubrzanje.

**Treće, pretpostavljeni trošak generalizacije ne postoji na ovoj razini težine zadatka.**
Raspoređivanje prepreka iznova u svakoj epizodi u potpunosti onemogućuje pamćenje njihovih
položaja, a lovca pritom ne stoji ništa. Najizravnije je tumačenje da je politika lovca i u Fazi A
bila reaktivna navigacija oko zaklona koji trenutačno opaža, a ne plan vezan uz određenu
geometriju — što raycast opažanja u kombinaciji s horizontom planiranja od približno sto odluka
očito dostatno podupiru.

**Četvrto, nasumičnost nije destabilizirala treniranje, nego suprotno.** Faza B ima **najuži raspon
među sjemenima od svih uvjeta u ovom radu**: 0,001 za stopu hvatanja i 34 za ELO razliku, naspram
0,005 odnosno 46 pri fiksnom rasporedu i 0,018 odnosno 54 u otvorenoj areni, a osobito naspram
izrazito bimodalnog ponašanja pri γ = 0,995. Mijenjanje rasporeda iz epizode u epizodu ponaša se,
dakle, kao blagi regularizator utrke u samoigri, a ne kao dodatni izvor šuma.

#### 6.6.3 Što Faza B zatvara, a što ostavlja otvorenim

Faza B **zatvara pitanje valjanosti** koje je pratilo glavni nalaz Faze A. Tvrdnja da fiksne
prepreke lovca ne stoje gotovo ništa pri γ ≥ 0,95 nosila je izričitu ogradu da se zapamćene
putanje obilaska ne mogu isključiti. Sada se mogu. Nalaz time prelazi iz uvjetnoga u utvrđeni:
**u ovoj areni i pri ovoj težini zadatka zaklon ne pomaže bjeguncu, bez obzira na to mijenja li se
raspored zaklona ili ne.**

Faza B **ne zatvara** dvije stvari. Prva je već spomenuta interakcija između γ i rasporeda
prepreka, budući da je izvedena pri jednoj vrijednosti γ. Druga je razlika između tvrdnje da su
prepreke *nebitne* i tvrdnje da *ne koriste bjeguncu*; izmjereno je isključivo ovo drugo.

#### 6.6.4 Ograničenja (radi poštenoga prikaza)

**Zasićenje mjerne veličine glavno je ograničenje.** Stopa hvatanja iznosi približno 1,0 u sva tri
uspoređivana uvjeta, pa glavna mjerna veličina ima malu moć razlučivanja sitnih razlika. Tvrdnja
koju podatci podupiru glasi da **mjerljivoga** učinka nasumičnoga rasporeda nema, a ne da učinka
nema uopće. Za stvarnu bi razlučivost bio potreban teži zadatak — više prepreka, brži bjegunac ili
veća arena.

**ELO ocjena ne smije nositi usporedbu među uvjetima.** Kao što je navedeno među ograničenjima
Faze A, ELO je vezan uz samoigru pojedinoga pokretanja i nije kalibriran između pokretanja.
Prividni niz 1218 → 1242 → 1257 od otvorene arene preko fiksnih do nasumičnih prepreka stoga se
**ne smije** tumačiti kao „prepreke pomažu lovcu"; usporedbe među pokretanjima nose stopa hvatanja
i duljina epizode, a obje su ovdje zasićene ili opterećene drugim čimbenicima.

Prosječna duljina epizode u otvorenoj areni (53,7) povišena je zbog jednoga slabijeg sjemena
(`POCA_sparse_s2`, 77,5 koraka uz stopu hvatanja 0,982); bez njega je otvorena arena brža od obaju
uvjeta s preprekama, zbog čega se ni na temelju duljine epizode ne iznosi tvrdnja o usporedbi
uvjeta.

Skupina pokretanja s fiksnim rasporedom objedinjuje γ = 0,95, 0,99 i 0,995 kako bi dosegla četiri
pokretanja; strogo istovrsna ćelija pri γ = 0,99 i dalje sadrži **samo jedno** pokretanje. Dopuna
dvama dodatnim sjemenima razmotrena je i svjesno nije provedena: četiri pokretanja s fiksnim
rasporedom pri γ ≥ 0,95 međusobno se podudaraju unutar 0,005 stope hvatanja i 46 ELO bodova, a
pokretanje `g099_s1` nalazi se u sredini toga skupa, pa nije riječ o sumnjivom uzorku.

Sva su mjerenja provedena u jednoj geometriji okoline: jedna veličina arene, četiri stupa, jedna
veličina stupa. Tvrdnja da zaklon ne pomaže bjeguncu odnosi se na **tu** konfiguraciju.

**Naposljetku, protučinjenična osnovica algoritma MA-POCA i dalje je brojčano neaktivna.** Omjer
`Losses/Baseline Loss` i `Losses/Value Loss` iznosi 1,0017, 1,0052 i 1,0062 u trima sjemenima Faze
B. Ni prepreke ni njihovo nasumično raspoređivanje ne mijenjaju ono što je utvrđeno u potpoglavlju
6.2: pri veličini grupe jedan osnovica nema suigrača na kojega bi se uvjetovala, pa **nijedan
rezultat ovoga rada dobiven u postavu jedan na jedan ne može razlikovati MA-POCA od PPO-a**. To je
izravan razlog zbog kojega se istraživanje nastavlja proširenjem na timove.

---

## D. Popis priloga koje treba izraditi

| Oznaka | Graf | Pokretanja |
|---|---|---|
| Slika 6.24 | `Environment/Catch` | `POCA_sparse_obsR_g099_s1/s2/s3` |
| Slika 6.25 | `Environment/Group Cumulative Reward` | isto |
| Slika 6.26 | `Environment/Episode Length` | isto |
| Slika 6.27 | `Self-play/ELO` | isto, obje uloge |
| Slika 6.28 | `Policy/Entropy` | isto |
| Slika 6.29 | `Environment/Catch`, usporedno | `obsR_g099_s1/s2/s3` + `obsF_g099_s1` |

Tablice 6.7 i 6.8 gotove su za prijenos; brojčane vrijednosti odgovaraju onima u `docs/Theory.md`,
odjeljak „Phase B results".
