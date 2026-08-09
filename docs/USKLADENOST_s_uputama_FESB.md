# Usklađenost rada s „Uputama za pisanje diplomskog rada" (FESB, lipanj 2017.)

Uspoređeno: `ANALIZA KOMPETITIVNE INTERAKCIJE… .docx` (392 odlomka, 14 slika, 5 tablica,
30 jednadžbi, ≈ 41 stranica) protiv `Upute za pisanje diplomskog rada.doc`.
Nijedna izvorna datoteka nije mijenjana.

---

## 1. Pregled usklađenosti

| Zahtjev iz Uputa | Stanje |
|---|---|
| Naslovnica, Zadatak, Izjava, Sadržaj | ✅ svi prisutni, ispravnim redoslijedom |
| Numeracija stranica (dolje desno, od Uvoda) | ✅ prednji dio nenumeriran, ostatak numeriran |
| Margine 2,5 cm sa sve četiri strane | ✅ |
| Times New Roman, 12 pt, prored 1,5, obostrano poravnanje | ✅ |
| Naslov poglavlja 14 pt bold, VELIKIM SLOVIMA | ⚠️ veličina ✅, velika slova ❌ (4 od 8 poglavlja) |
| Potpoglavlje 1. razine 12 pt bold | ⚠️ postavljeno na **13 pt** |
| Najviše tri razine naslova | ⚠️ koristi se i četvrta razina (`Heading4`) |
| Svako poglavlje počinje na novoj stranici | ⚠️ samo 5 prijeloma stranice na 8 poglavlja |
| **Naslov tablice IZNAD tablice** | ❌ sve tablice imaju naslov ispod |
| **Numeracija tablica po poglavljima** (Tablica 2.1.) | ❌ koristi se `Tablica 1, 2, 3…` |
| **Numeracija slika po poglavljima** (Slika 2.1.) | ❌ koristi se `Slika 2-1` (crtica umjesto točke) |
| Naslov slike ispod slike, centrirano | ✅ |
| **Prijevod stranih pojmova na preuzetim slikama** | ❌ nijedna preuzeta slika nije prevedena |
| Referenca na kraju naziva preuzete slike | ⚠️ djelomično (3 od 6) |
| **Numeriranje formula (2.1), (2.2)…** | ❌ nijedna od 30 jednadžbi nije numerirana |
| Opis oznaka nakon formule („gdje je:") | ⚠️ neujednačeno |
| Decimalni zarez umjesto točke | ⚠️ nekoliko propusta (`0.5`, `99.4 %`) |
| **Format literature prema Uputama** | ❌ koristi se IEEE stil; 4 jedinice bez autora |
| **Neodređeno / treće lice množine, pasivne forme** | ❌ poglavlje 2 pisano u prvom licu množine |
| **Zaključak (do 2 str.)** | ❌ prazan |
| **Popis oznaka i kratica** | ❌ prazan |
| **Sažetak (do 1 str.), Ključne riječi (3–5)** | ❌ prazni |
| **Title, Summary, Keywords (engleski)** | ❌ prazni |
| Uvod do 2 stranice | ✅ 2 stranice (ali sadržajno neispravan) |
| **Dokumentacija programske podrške** | ❌ nema isječaka koda ni priloga |
| Uravnotežen opseg poglavlja | ⚠️ pogl. 3 = 15 str., pogl. 4 i 5 = po 4 str. |

---

## 2. Što je već dobro

Formalna osnova dokumenta postavljena je ispravno i to je najlakši dio za zabrljati:

- **Margine, font, prored i poravnanje točno odgovaraju Uputama** — 2,5 cm sa sve četiri
  strane, Times New Roman 12 pt, prored 1,5, obostrano poravnanje.
- **Numeracija stranica riješena je ispravno** — dokument je podijeljen na dvije sekcije, pa
  naslovnica, zadatak, izjava i sadržaj nisu numerirani, a numeracija kreće od Uvoda.
- **Prednji dio rada je kompletan i ispravnog redoslijeda** — naslovnica prema Prilogu 1,
  zadatak diplomskog rada, izjava o autorstvu, pa sadržaj s brojevima stranica.
- **Naslovi slika su ispod slika i centrirani**, kako Upute traže.
- **Stil `Caption` je kurzivan**, što odgovara zahtjevu za naslove tablica.
- **Referenciranje u tekstu u uglatim zagradama `[1]`** je ispravno i dosljedno provedeno.
- **Uvod ne prelazi dvije stranice**, kako Upute izričito traže.

---

## 3. Obavezni dijelovi koji nedostaju

Upute u poglavlju „STRUKTURA DIPLOMSKOG RADA" nabrajaju dijelove koje rad **mora** sadržavati.
Sljedeći postoje kao naslovi, ali su prazni:

| Dio | Zahtjev iz Uputa |
|---|---|
| **Zaključak** | „sažimaju se rezultati diplomskog rada… ne bi trebao biti duži od dvije stranice" |
| **Popis oznaka i kratica** | „naročito važno kad se u radu koristi puno oznaka i kratica" — rad koristi ~25 kratica, dakle nije opcionalno |
| **Sažetak** | „kratki pregled svog diplomskog rada (u obimu do jedne stranice)" |
| **Ključne riječi** | „3 do 5 riječi" |
| **Title** | naslov rada na engleskom |
| **Summary** | sažetak na engleskom |
| **Keywords** | ključne riječi na engleskom |

Dodatno, Upute traže: *„Diplomski rad treba sadržavati opis izvedbe sklopa ili programske
podrške s potpunom dokumentacijom. Takva dokumentacija se obavezno prilaže u elektroničkoj
formi, ali može biti i u dodacima."*

Rad trenutno **nema nijedan isječak koda**, a naslov „Ostali prilozi i dokumentacija" je prazan,
kao i „Kazalo kodova". Za rad čiji je cjelokupni doprinos programska implementacija to je
propust koji će mentor gotovo sigurno primijetiti. Preporuka: 4–6 kratkih listinga u tekstu
(potencijalna funkcija Φ(s) i član F u `TagAgent`, `AddGroupReward` + `EndGroupEpisode`, YAML
konfiguracija `poca` trenera sa `self_play` blokom, `StatsRecorder` za `Environment/Catch`) te
Dodatak A s poveznicom na repozitorij i popisom priloženih datoteka.

### Redoslijed završnih dijelova

Upute propisuju redoslijed: **Literatura → Popis oznaka i kratica → Sažetak → Ključne riječi →
Title → Summary → Keywords → Dodatak A, B…**

Rad ima: Literatura → Kazalo slika, tablica i kodova → Popis oznaka i kratica → Ostali prilozi
i dokumentacija → Sažetak/Abstract.

Dvije izmjene: **Sažetak mora doći prije priloga**, a „Kazalo slika, tablica i kodova" nije
predviđeno Uputama — nije zabranjeno, ali ga je bolje smjestiti neposredno iza Sadržaja ili
premjestiti u Dodatak, umjesto između Literature i Popisa oznaka.

---

## 4. Slike, tablice i formule

### 4.1 Tablice — dvije sustavne pogreške

Upute: *„Svaka tablica mora imati naslov i mora biti numerirana. **Naslov tablice dolazi iznad
nje.** Koristiti zasebnu numeraciju u svakom poglavlju (npr. Tablica 2.1. Naslov tablice)."*

U radu su **svi naslovi tablica ispod tablica**, a numeracija je kroz cijeli rad (`Tablica 1`
do `Tablica 5`) umjesto po poglavljima. Ispravno bi bilo:

| Sadašnje | Ispravno |
|---|---|
| Tablica 1 Struktura nagrade i epizoda | **Tablica 4.1.** Struktura nagrade i epizoda |
| Tablica 2 Dizajn sparse i shaped ruke treniranja | **Tablica 5.1.** Dizajn rijetke i oblikovane ruke treniranja |
| Tablica 3 Loss term metrika | **Tablica 6.1.** Vrijednosti funkcija gubitka |
| Tablica 4 Rezultati 5M treninga FAZE A | **Tablica 6.2.** Rezultati rijetke ruke pri 5 milijuna koraka |
| *(bez naslova)* | **Tablica 6.3.** Rezultati oblikovane ruke pri 5 milijuna koraka |

Uz to: naslov `Tablica 3 Loss term metrika` miješa engleski i hrvatski; Upute traže hrvatski
književni jezik. Peta tablica (oblikovana ruka) uopće nema naslov.

### 4.2 Slike — numeracija i neprevedeni pojmovi

Numeracija koristi **crticu** (`Slika 2-1`) umjesto **točke** (`Slika 2.1.`). Sitnica, ali
pojavljuje se u svim naslovima i u Kazalu slika, pa je izmjena mehanička (Find & Replace).

Ozbiljniji je ovaj zahtjev: *„Ukoliko se slika preuzima iz literature na kojoj se nalaze pojmovi
na nekom od stranih jezika, **značenje tih pojmova mora biti navedeno i na hrvatskom jeziku**."*

Sve preuzete slike u radu su na engleskom i nijedna nije prevedena:

| Slika | Što treba prevesti |
|---|---|
| Slika 2-1 (interakcija agent–okolina) | oznake `agent`, `environment`, `action`, `reward`, `state` |
| Slika 3-1 (PPO pseudokod) | cijeli pseudokod — dodaj hrvatski prijevod naredbi ispod ili u naslovu slike |
| Slika 3-2 (MuJoCo usporedba) | nazivi okruženja i osi |
| Slika 3-4 (testna okruženja a–d) | nazivi okruženja; djelomično je već objašnjeno u tekstu ispod, ali treba i uz sliku |
| Slika 3-5 (usporedba MA-POCA/COMA/PPO) | legenda i oznake osi |

Najjednostavnije rješenje bez ponovnog crtanja: proširiti naslov slike popisom prijevoda, npr.
*„Slika 3.2. Usporedba algoritama na okruženjima MuJoCo (eng. `Hopper-v1` — skakač, `Walker2d-v1`
— dvonožni hodač…). Preuzeto iz: [9]"*.

Također, reference nedostaju u naslovima slika 3-1, 3-2 i 3-4, iako su sve preuzete iz literature.
Slika 3-5 navodi „(Cohen i sur., 2022)" bez broja u uglatoj zagradi — treba dodati `[11]` radi
dosljednosti s ostatkom rada.

Preostaju i dvije neoznačene slike: jedna u potpoglavlju 4.6.1 i jedna neposredno ispod naslova
LITERATURA. Obje treba ili opremiti naslovom i brojem, ili ukloniti.

### 4.3 Formule — nijedna nije numerirana

Upute: *„Formule se obilježavaju brojem u običnoj zagradi, prvi broj je redni broj poglavlja, a
drugi broj je broj formule u tom poglavlju."*

Rad sadrži **30 jednadžbi i nijedna nema broj**. To otežava i vlastito pozivanje na njih — na
nekoliko mjesta u tekstu piše „prema formuli" bez oznake, a u potpoglavlju 5.4 ostala su i
prazna mjesta: *„prema formuli ."*, *„dok pri  pada na svega 5 odluka"* i *„u sve tri testirane
konfiguracije (; ; )"*. Te tri rečenice treba provjeriti — izgleda da su jednadžbe ili
vrijednosti γ ispale iz teksta.

Uz numeraciju, Upute traže i kratak opis oznaka nakon svake formule („gdje je: …"). Rad to radi
neujednačeno — npr. kod definicije MDP-a i potencijalne funkcije Φ(s) opisi postoje, kod
Bellmanovih izraza u 2.2.4 ne.

### 4.4 Veličine i jedinice

Upute pozivaju na Zakon o mjeriteljstvu: oznake veličina kurzivom, jedinice uspravno, decimalni
**zarez**, razmak između iznosa i jedinice.

Rad uglavnom ispravno koristi decimalni zarez (−0,001; 1890,7; 0,99), ali ima propusta:
`coef = 0.5` u potpoglavlju 3.4.6 i `99.4%` u Uvodu. Kod postotka Upute traže da se znak `%`
piše zajedno s brojem — provjeriti dosljednost kroz cijeli rad.

---

## 5. Jezik i stil

Upute su izričite: *„Rad treba pisati hrvatskim književnim jezikom u neodređenom ili trećem licu
množine, a ukoliko je to moguće, potrebno je pisati u pasivnim formama."*

Poglavlje 2 sustavno krši ovo pravilo obraćajući se čitatelju u prvom licu množine:

| Sadašnje | Prijedlog |
|---|---|
| „**Recimo** da u nekom nepoznatom okruženju **imamo** agenta" | „Neka se u nepoznatom okruženju nalazi agent" |
| „Formalno **definirajmo** ključne koncepte" | „U nastavku se formalno definiraju ključni koncepti" |
| „**Zamislimo** scenarij u kojem…" | „Promotrimo li scenarij…" → bolje: „U scenariju u kojem…" |
| „**Označimo** stanje, akciju i nagradu…" | „Stanje, akcija i nagrada označavaju se…" |
| „**Možemo reći** da je vrijednosna funkcija kvantitativna ocjena stanja" | „Vrijednosna funkcija predstavlja kvantitativnu ocjenu stanja" |

Ukupno je pronađeno 8 takvih mjesta; sva su u poglavlju 2, dok su poglavlja 4–6 pisana ispravno.

**Miješanje jezika u naslovima.** Naslovi potpoglavlja 6.3.1 („Sparse arm – emergentni progon")
i 6.3.2 („Shaped arm – „proximity farming"") su dvojezični. Zahtjev za hrvatskim književnim
jezikom odnosi se i na naslove. Prijedlog: *„6.3.1. Rijetka nagrada — emergentni progon"* i
*„6.3.2. Oblikovana nagrada — oblikovno farmiranje i inverzija rezultata"*, uz engleski naziv u
zagradi kod prvog spominjanja u tekstu.

**Nedosljedna kratica za engleski.** Rad koristi i `eng.` i `engl.`; Upute u primjeru popisa
kratica navode `engl`. Odabrati jedno i provesti kroz cijeli rad.

**Naslov poglavlja 3** glasi „Pregled korištenih rml Algoritama" — mala slova u kratici,
nepotrebno veliko slovo u „Algoritama", i nije napisan velikim slovima kako Upute traže za
naslove poglavlja.

---

## 6. Literatura

Upute propisuju format:

```
Prezime1, I1.; Prezime2, I2.: "Naslov knjige", Izdavač, mjesto izdavanja, godina.
Prezime, I.: "Naslov", s Interneta, http://adresa.xx, točan datum.
```

Rad koristi **IEEE stil** (inicijali ispred prezimena) za jedinice [5]–[13], a jedinice [1]–[4]
uopće nemaju autora. Upute za internetske izvore izričito traže: *„uz čiju adresu **svakako mora
biti naveden autor** citiranog materijala i datum"*.

| Jedinica | Problem | Prijedlog |
|---|---|---|
| [1] NVIDIA povijest | nema autora | dodati autora članka ili instituciju kao autora |
| [2] Lee Sedol / AlphaGo | nema autora | `Gomagic: "Lee Sedol and AlphaGo…", s Interneta, …` |
| [3] Dota 2 | nema autora, nema „s Interneta" | `OpenAI i dr.: "Dota 2 with Large Scale Deep Reinforcement Learning", s Interneta, …` |
| [4] Lilian Weng | nema autora | `Weng, L.: "A (Long) Peek into Reinforcement Learning", s Interneta, …` |
| [5]–[12] | IEEE format | preurediti u `Prezime, I.: "Naslov", Izdavač, mjesto, godina.` |
| [13] ML-Agents dokumentacija | URL sadrži cijeli upit za pretraživanje (~200 znakova) | skratiti na osnovnu adresu stranice |

Redoslijed navođenja prati redoslijed pojavljivanja u tekstu ✅ — to je već ispravno.

---

## 7. Struktura poglavlja

### 7.1 Razine naslova

Upute: *„Ne smije se pretjerivati razinama potpoglavlja… Uobičajeno rabiti do tri razine naslova
(poglavlje i dva potpoglavlja)."*

Rad koristi i četvrtu razinu (stil `Heading4`) na pet mjesta: „Oblikovanje zasnovano na
potencijalu (PBS)", „Invariantnost politike i njezina ograničenja", „Opis testnih okruženja",
„Rezultati". Budući da nisu numerirani, ne pojavljuju se u Sadržaju, pa formalno prolaze — ali
ih je čišće ili podići na treću razinu, ili pretvoriti u podebljani uvodni tekst odlomka.

### 7.2 Prijelomi stranica

Upute: *„Svako novo poglavlje treba početi na novoj stranici."* U dokumentu postoji 5 prijeloma
stranice, a poglavlja na prvoj razini ima 8 (Uvod, 2, 3, 4, 5, 6, Zaključak, Literatura). Umjesto
ručnih prijeloma preporučuje se uključiti *Page break before* u definiciji stila `Heading 1` —
tada se pravilo primjenjuje samo od sebe.

### 7.3 Veličina naslova potpoglavlja

Stil `Heading 2` postavljen je na **13 pt**; Upute traže 12 pt bold za prvu razinu potpoglavlja
i 12 pt bez podebljanja za drugu.

### 7.4 Uravnoteženost poglavlja

Upute: *„Poglavlja bi trebala biti uravnoteženog obima."* Prema Sadržaju:

| Poglavlje | Stranice | Opseg |
|---|---|---|
| 1 Uvod | 1–2 | 2 |
| 2 Teorijska podloga | 3–7 | 5 |
| **3 Pregled RML algoritama** | **8–22** | **15** |
| 4 Okruženje i implementacija | 23–26 | 4 |
| 5 Eksperimentalni dizajn | 27–30 | 4 |
| 6 Empirijski rezultati | 31–36 | 6 |

Poglavlje 3 je gotovo četiri puta opsežnije od poglavlja 4 i 5, a riječ je o preuzetoj teoriji.
Istovremeno, poglavlja koja nose vlastiti doprinos (4, 5 i 6) zajedno imaju 14 stranica — manje
od teorijskog pregleda. To je disproporcija koju mentor vjerojatno komentira, i rješava se
prirodno kad se u poglavlje 6 unesu tablice i slike koje su već pripremljene (v. datoteku
`EVALUACIJA_diplomskog_rada.md`), čime poglavlja 4–6 narastu na realnih 20-ak stranica.

---

## 8. Redoslijed izmjena

**A — obavezno prije predaje (rad bez ovoga formalno nije potpun):**

1. Napisati Zaključak (do 2 str.), Sažetak (do 1 str.), Ključne riječi (3–5), Title, Summary,
   Keywords.
2. Popuniti Popis oznaka i kratica, abecednim redom.
3. Dodati dokumentaciju programske podrške — isječke koda u tekstu i Dodatak A.
4. Prepisati Uvod (odlomci o Apache Kafki i Taxi aplikacijama nisu iz ovog rada).
5. Premjestiti Sažetak ispred priloga.

**B — mehaničke izmjene, brzo se provode:**

6. Premjestiti naslove tablica iznad tablica; prenumerirati tablice i slike po poglavljima
   (`Tablica 4.1.`, `Slika 3.2.`); dodati naslov petoj tablici.
7. Numerirati sve formule po poglavljima; popuniti tri rečenice s praznim jednadžbama u 5.4.
8. Preurediti popis literature u format iz Uputa; dodati autore za [1]–[4].
9. Naslove poglavlja napisati velikim slovima; `Heading 2` postaviti na 12 pt; uključiti *Page
   break before* u stilu `Heading 1`.
10. Ispraviti decimalne točke (`0.5`, `99.4 %`) u decimalne zareze.

**C — sadržajne dorade:**

11. Prevesti strane pojmove na preuzetim slikama i dodati reference u naslove slika 3.1, 3.2, 3.4.
12. Preurediti poglavlje 2 u neodređeno/treće lice množine (8 mjesta).
13. Prevesti naslove 6.3.1 i 6.3.2 na hrvatski.
14. Ujednačiti `eng.` / `engl.` kroz cijeli rad.
15. Na kraju: `Ctrl+A → F9` za osvježavanje Sadržaja, Kazala slika i Kazala tablica.
