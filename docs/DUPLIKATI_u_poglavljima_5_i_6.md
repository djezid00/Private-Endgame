# Ponavljanja u poglavljima 5 i 6 — što obrisati, što premjestiti

**Analizirano 18.8.2026.** protiv trenutnog `.docx` (784 elementa, 32 slike, 11 tablica).
Dojam o ponavljanju je **točan**, i ima dva različita uzroka:

1. **Dvije verzije istog teksta ostale su jedna ispod druge** (potpoglavlje 5.5) — očito je novija
   verzija zalijepljena, a starija nije obrisana.
2. **Rezultati su napisani u poglavlju 5**, koje se zove *Dizajn eksperimenta*, pa se isti nalazi
   nužno ponavljaju u poglavlju 6, koje se zove *Empirijski rezultati*.

Ovo nije problem stila nego strukture: čitatelj istu brojku dobiva više puta i ne zna koje je
mjesto mjerodavno. Ispod je popis po mjestima, s najmanjim mogućim zahvatom — **brisanje i
premještanje, bez novog pisanja**, osim tri kratke vezne rečenice koje su izričito navedene.

> **Snalaženje:** odlomci se ovdje citiraju po početku rečenice, a ne po broju, kako bi upute
> ostale valjane i nakon što se dokument izmijeni.

---

## 1. Najveće ponavljanje: potpoglavlje 5.5 sadrži cijeli eksperiment dvaput

Potpoglavlje „Eksperiment 3: Utjecaj diskontnog faktora γ" sadrži **dvije potpune verzije istog
sadržaja**, jednu iza druge:

| Verzija | Početak teksta | Sadržaj | Stanje |
|---|---|---|---|
| **Starija** | „Diskontni faktor 𝛾 određuje efektivni horizont planiranja agenta **prema formuli .**" + „Zaključak eksperimenta: Rezultati su u potpunosti potvrdili predikciju… **(; ; )**" | horizont → predikcija → rezultat | **oštećena** — tri prazna mjesta na kojima su ispale jednadžbe i vrijednosti γ |
| **Novija** | „Diskontni faktor γ određuje koliko daleko unaprijed agent uopće „vidi" posljedice svojih poteza…" (pet odlomaka, do „…neovisnim o duljini horizonta planiranja.") | isto, ali potpunije | **ispravna** — sadrži izvod horizonta preko `DecisionPeriod` i `Fixed Timestep`, formulu stalnog člana, uvjet opovrgavanja i brojke po γ |

**Zahvat: obriši stariju verziju u cijelosti** (oba odlomka). Novija je bolja u svakom pogledu i
ništa se brisanjem ne gubi.

Ovim se **usput rješava i stara zamjerka o praznim jednadžbama.** Sva tri prazna mjesta
(„prema formuli .", „dok pri  pada na svega 5 odluka", „u sve tri testirane konfiguracije (; ; )")
nalaze se **isključivo u starijoj verziji**. Nakon brisanja nema se što popunjavati.

---

## 2. Rezultati u poglavlju 5 (dizajn) — premjestiti u poglavlje 6

Poglavlje 5 opisuje *kako je eksperiment postavljen*. Trenutno sadrži i *što je iz njega izašlo*, na
dva mjesta.

### 2.1 Rezultati γ pretraživanja u 5.5

Tri odlomka koji počinju s:

- „**Rezultat: Zamka parazitiranja se nije nikako smanjila…**"
- „**Prava je vrijednost pokusa, međutim, u brojci koju agent jest postigao…**"
- „**Naposljetku, isti podaci otkrivaju i kako je lovac parazitirao…**"

su rezultati, a ne dizajn.

**Zahvat: premjesti ta tri odlomka u 6.5.3** („Kratkovidnost naspram nestabilnosti: uloga γ"),
neposredno iza odlomka koji završava s „…između zone miopije (< 0,95) i zone nestabilnosti
(≥ 0,995)." U 5.5 ostaje samo opis pokusa i predikcija.

> **Ovo popravlja i jednu pogrešnu uputu.** U 6.4.2 stoji: *„Kvantitativna potvrda ovog mehanizma,
> dobivena mjerenjem prikupljene nagrade pri različitim vrijednostima γ, iznesena je u potpoglavlju
> 6.5.3."* Ta potvrda trenutno **nije** u 6.5.3 nego u 5.5, pa uputa vodi na krivo mjesto. Nakon
> premještanja rečenica postaje točna bez ikakve izmjene.

Vezna rečenica na kraju 5.5, umjesto premještenog teksta:

> Rezultati ovog pokusa izneseni su u potpoglavlju 6.5.3.

### 2.2 Rezultati Faze A u 5.6

Tri odlomka koji počinju s „**Rezultati RQ-A pokazali su monoton rast…**", „**Rezultat RQ-C bio je
negativan…**" i „**Zaključak: γ=0,99 potvrđen je kao empirijski optimalan izbor…**" ponavljaju ono
što poglavlje 6 već iznosi:

| Odlomak u 5.6 | Gdje isto već postoji u poglavlju 6 |
|---|---|
| RQ-A: „stopa hvatanja rasla je s 0,86 … do 1,00 pri γ=0,95", bimodalnost na 0,995 | **Tablica 6.6** (svi brojevi, po sjemenima) + 6.5.3 |
| RQ-C: „fiksne prepreke praktički nisu promijenile ishod igre" | **6.5.4** („Prepreke i intuicija o prednosti bjegunca") |
| Zaključak: „γ=0,99 … najbrže učenje i najviši plafon performansi" | **6.5.3**, odlomak „Empirijski optimum na γ = 0,99 nije slučajan…" |

**Zahvat: obriši sva tri odlomka iz 5.6.** Poglavlje 6 iznosi iste nalaze detaljnije i uz tablicu.
Zadrži u 5.6 samo ono što je stvarno dizajn: obrazloženje podjele na faze (tri odlomka koji počinju
s „Prije opisa provedenih konfiguracija…") i opis matrice od devet konfiguracija.

Vezna rečenica na kraju 5.6:

> Rezultati Faze A izneseni su u potpoglavljima 6.5.3 i 6.5.4.

---

## 3. Ponavljanja unutar poglavlja 6

Ovdje je ponavljanje **manje ozbiljno** — dio je legitiman jer je 6.5 *rasprava*, pa smije podsjetiti
na nalaz. Problem nastaje kada rasprava iznosi isti argument jednakom dužinom kao i odjeljak s
rezultatima, pa se čita kao kopija.

### 3.1 „Zamka ima dva uzroka" — napisano dvaput, gotovo jednako

- **6.2** (POCA vs PPO), odlomak „Prvi dio hipoteze time je potvrđen…": iznosi oba uzroka, uzročnu
  provjeru (0,01 → 0,12) i zaključak da je rijetka nagrada robusno rješenje.
- **6.5.2** („PBS zamka: dva odvojena uzroka"): iznosi **iste** dvije točke i **istu** brojku
  „0,12 naspram PPO-ovih 0,98".

**Zahvat (najmanji mogući):** 6.2 je mjesto gdje su podaci — ondje argument ostaje u cijelosti.
U **6.5.2 izbaci ponovljenu brojku** „0,12 naspram PPO-ovi 0,98 stope hvatanja" i zamijeni je uputom
„(v. Tablicu 6.2)". Rasprava tada tumači nalaz umjesto da ga prepričava, a ušteda je samo jedna
rečenica — ostatak odlomka o algoritamskoj osjetljivosti ostaje netaknut.

### 3.2 Inverzija 400k → 5M — ispričana tri puta

Ista poanta pojavljuje se u:

1. **6.3** — „Oblikovanje nagrade u 400k koraka pokazalo je ~3× veći ELO razmak…" (ovdje su podaci,
   legitimno)
2. **6.4.2** — „Drugo i možda najvrjednije, validacija na kratkom horizontu dovela bi do suprotnog
   zaključka… Inverzija poretka između kratkog i punog budžeta time postaje metodološki rezultat po
   sebi."
3. **6.5.1** — „…validacijski pokus pri 400k koraka sugerirao je suprotno, tj. da je shaped arm ~3×
   učinkovitije. Da je eksperiment završen na ovoj razini, zaključak bi bio kriv."

Točke 2 i 3 govore istu stvar gotovo istim riječima.

**Zahvat: zadrži u 6.5.1, obriši iz 6.4.2.** U 6.5.1 je metodološka lekcija tematski na mjestu, a
6.4.2 time ostaje usredotočen na mehanizam farmiranja. Nakon brisanja u 6.4.2 nabrajanje ima dvije
stavke umjesto tri, pa **uvodnu rečenicu popravi u „…i to iz dvaju razloga"**, a „Treće, rijetka
terminalna nagrada…" postaje „Drugo, …".

### 3.3 Sitnije: rezultat rijetke ruke naveden triput — ostaviti

Stopa hvatanja ~1,00 i ELO ~1890 pojavljuju se u **Tablici 6.4**, u 6.4.2 („Za usporedbu, rijetka
ruka… hvata Runnera u gotovo svakoj epizodi") i u 6.5.1 („doseglo je stopu hvatanja ~1,00 i ELO
Chasera ~1890"). Ovo je **prihvatljivo**: tablica su podaci, 6.4.2 je usporedba, 6.5.1 je zaključak.
Ostaviti kako jest.

---

## 4. Dvije pokvarene unakrsne upute

Nisu ponavljanje, ali su u istom području i lako se previde. U 5.2.3 i 5.2.4 Word prikazuje
**„potpoglavlju 0"** i **„poglavlje 0"** umjesto broja:

- „…što je mehanizam otkriven u potpoglavlju **0**."
- „…kako pokazuje poglavlje **0**, najvažniji pojedinačni nalaz ovog rada."

Obje trebaju pokazivati na **6.4.2**. Ponovno umetni unakrsnu uputu (*Insert → Cross-reference*) ili
upiši broj ručno.

---

## 5. Redoslijed izvođenja

1. Obriši stariju verziju u 5.5 (§1) — najveći učinak, ništa se ne gubi, usput nestaju prazne jednadžbe.
2. Premjesti tri odlomka s rezultatima iz 5.5 u 6.5.3 (§2.1) i dodaj veznu rečenicu.
3. Obriši tri odlomka s rezultatima iz 5.6 (§2.2) i dodaj veznu rečenicu.
4. Izbaci ponovljenu brojku u 6.5.2 (§3.1).
5. Obriši priču o inverziji iz 6.4.2 i popravi nabrajanje na dvije stavke (§3.2).
6. Popravi dvije upute „0" (§4).

Nakon toga poglavlje 5 sadrži isključivo dizajn i predikcije, poglavlje 6 isključivo rezultate i
raspravu, a svaka se brojka pojavljuje na jednom mjerodavnom mjestu.
