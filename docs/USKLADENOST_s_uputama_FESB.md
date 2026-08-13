# Usklađenost rada s „Uputama za pisanje diplomskog rada" (FESB, lipanj 2017.)

**Revizija 2 — 13.8.2026.** Uspoređeno: `ANALIZA KOMPETITIVNE INTERAKCIJE… .docx`
(725 odlomaka, 8 tablica, 27 slika, 3 isječka koda, ~30 jednadžbi) protiv
`Upute za pisanje diplomskog rada.doc`. Prethodna revizija: 392 odlomka, 14 slika, 5 tablica.
Nijedna izvorna datoteka nije mijenjana.

---

## 1. Pregled usklađenosti

| Zahtjev iz Uputa | Stanje |
|---|---|
| Naslovnica, Zadatak, Izjava, Sadržaj | ✅ svi prisutni, ispravnim redoslijedom |
| Numeracija stranica (dolje desno, od Uvoda) | ✅ prednji dio nenumeriran, ostatak numeriran |
| Margine 2,5 cm sa sve četiri strane | ✅ |
| Times New Roman, 12 pt, prored 1,5, obostrano poravnanje | ✅ |
| Naslov poglavlja 14 pt bold, VELIKIM SLOVIMA | ✅ **riješeno** — svih 7 naslova prve razine velikim slovima |
| Potpoglavlje 1. razine 12 pt bold | ⚠️ i dalje **13 pt** |
| Najviše tri razine naslova | ⚠️ i dalje se koristi četvrta razina (5 mjesta) |
| Svako poglavlje počinje na novoj stranici | ⚠️ nije dosljedno provedeno |
| **Naslov tablice IZNAD tablice** | 🟡 **djelomično** — Tablice 5, 6, 7 ispravno iznad; Tablice 1–4 i dalje ispod |
| **Numeracija tablica po poglavljima** (Tablica 2.1.) | ❌ i dalje linearno `Tablica 1`…`7` |
| **Numeracija slika po poglavljima** (Slika 2.1.) | ✅ **riješeno** — `Slika 2.1` … `Slika 6.17` |
| Naslov slike ispod slike, centrirano | ✅ |
| **Prijevod stranih pojmova na preuzetim slikama** | ❌ nijedna preuzeta slika nije prevedena |
| Referenca na kraju naziva preuzete slike | ⚠️ djelomično |
| **Numeriranje formula (2.1), (2.2)…** | ❌ nijedna od ~30 jednadžbi nije numerirana |
| Opis oznaka nakon formule („gdje je:") | ⚠️ neujednačeno |
| Decimalni zarez umjesto točke | ⚠️ **pogoršano** — nova Tablica 5 cijela u decimalnim točkama; `0.5`, `99.4 %` i dalje u tekstu |
| **Format literature prema Uputama** | ❌ IEEE stil; jedinice [1]–[4] bez autora |
| **Neodređeno / treće lice množine, pasivne forme** | ❌ poglavlje 2 i dalje u prvom licu množine (8 mjesta) |
| **Zaključak (do 2 str.)** | ❌ prazan |
| **Popis oznaka i kratica** | ✅ **riješeno** — 25 stavki, abecedno |
| **Sažetak (do 1 str.), Ključne riječi (3–5)** | ❌ prazni |
| **Title, Summary, Keywords (engleski)** | ❌ prazni |
| Uvod do 2 stranice | ✅ opseg da — ali sadržajno i dalje neispravan (v. §3) |
| **Dokumentacija programske podrške** | 🟡 **djelomično riješeno** — 3 isječka koda u tekstu; „Ostali prilozi i dokumentacija" i dalje prazno |
| Uravnotežen opseg poglavlja | ✅ **bitno poboljšano** — poglavlja 4–6 znatno proširena, disproporcija prema pogl. 3 uklonjena |

---

## 2. Što je riješeno od prošle revizije

- **Naslovi poglavlja velikim slovima** — svih 7 (`UVOD`, `TEORIJSKA PODLOGA`,
  `PREGLED KORIŠTENIH RML ALGORITAMA`, `OKRUŽENJE I IMPLEMENTACIJA`, `DIZAJN EKSPERIMENTA`,
  `EMPIRIJSKI REZULTATI`, `ZAKLJUČAK`). Time je usput riješena i nedosljedna kapitalizacija
  naslova poglavlja 3 („Pregled korištenih **rml** Algoritama").
- **Numeracija slika po poglavljima s točkom** — `Slika 2.1` umjesto `Slika 21`. Provedeno kroz
  cijeli rad, uključujući 17 novih slika u poglavlju 6.
- **Popis oznaka i kratica** — popunjen, 25 stavki, abecednim redom, s hrvatskim objašnjenjem i
  engleskim izvornikom u zagradi.
- **Dokumentacija programske podrške** — tri isječka koda (`Kod 31`, `Kod 41`, `Kod 42`) plus
  funkcija `OnAgentTagged` u §4.5. Time „Kazalo kodova" konačno ima što prikazati (nakon `F9`).
- **Naslovi tablica iznad tablice** — nove Tablice 5, 6 i 7 slijede pravilo.
- **Uravnoteženost poglavlja** — poglavlje 4 (konfiguracija, scena, kod) i poglavlje 6
  (17 slika, 4 tablice) znatno su narasla, pa vlastiti doprinos više nije kraći od preuzete teorije.

---

## 3. Obavezni dijelovi koji i dalje nedostaju

Upute u poglavlju „STRUKTURA DIPLOMSKOG RADA" nabrajaju dijelove koje rad **mora** sadržavati.
Sljedeći i dalje postoje samo kao prazni naslovi:

| Dio | Zahtjev iz Uputa |
|---|---|
| **Zaključak** | „sažimaju se rezultati diplomskog rada… ne bi trebao biti duži od dvije stranice" |
| **Sažetak** | „kratki pregled svog diplomskog rada (u obimu do jedne stranice)" |
| **Ključne riječi** | „3 do 5 riječi" |
| **Title / Summary / Keywords** | naslov, sažetak i ključne riječi na engleskom |
| **Ostali prilozi i dokumentacija** | Dodatak A s poveznicom na repozitorij i popisom priloženih datoteka |

**Uvod i dalje sadrži tekst iz tuđeg rada.** Odlomci 115–120 opisuju Taxi aplikacije, Apache Kafku,
WebSocket, Firebase i „FUBAR Taxi". Šest odlomaka koji nemaju veze s ovim radom. Uz to, odlomak 113
sadrži pokvarenu Word referencu **„Error! Reference source not found."** umjesto izvora [3].
Ovo je i dalje **najveći rizik za dojam pri predaji** — mentor to primijeti u prvoj minuti.

### Redoslijed završnih dijelova

Upute propisuju: **Literatura → Popis oznaka i kratica → Sažetak → Ključne riječi → Title → Summary
→ Keywords → Dodatak A, B…**

Rad ima: Literatura → Kazalo slika, tablica i kodova → Popis oznaka i kratica → Ostali prilozi i
dokumentacija → Sažetak/Abstract. Sažetak treba doći **prije** priloga; „Kazalo slika, tablica i
kodova" nije predviđeno Uputama pa ga je bolje smjestiti iza Sadržaja ili u Dodatak.

---

## 4. Slike, tablice i formule

### 4.1 Tablice

Numeracija je i dalje **linearna kroz cijeli rad** (`Tablica 1`–`7`), dok su slike u međuvremenu
prešle na numeraciju **po poglavljima** (`Slika 6.1`). Rad je time **nedosljedan sam sa sobom** — to
je sada uočljivije nego prije, jer se obje sheme pojavljuju na istoj stranici. Predloženo:

| Sadašnje | Ispravno |
|---|---|
| Tablica 1 Struktura nagrade i epizoda | **Tablica 4.1.** |
| Tablica 2 Pregled mjernih veličina | **Tablica 5.1.** |
| Tablica 3 Dizajn sparse i shaped ruke treniranja | **Tablica 5.2.** |
| Tablica 4 Loss term metrika | **Tablica 6.1.** Vrijednosti funkcija gubitka |
| Tablica 5 400k validacija: sparse vs shaped | **Tablica 6.2.** |
| Tablica 6 Rezultati rijetke (sparse) ruke, 5M | **Tablica 6.3.** |
| Tablica 7 Rezultati oblikovane (shaped) ruke, 5M | **Tablica 6.4.** |
| *(γ-tablica, bez naslova)* | **Tablica 6.5.** Rezultati Faze A — devet konfiguracija |

Naslov `Tablica 4 Loss term metrika` i dalje miješa engleski i hrvatski.
**Osma tablica (γ-tablica u §6.4.3) uopće nema naslov** pa neće ući u Kazalo tablica.

### 4.2 Slike — numeracija riješena, ostaje prijevod

Numeracija je ispravljena. Ostaje zahtjev: *„Ukoliko se slika preuzima iz literature na kojoj se
nalaze pojmovi na nekom od stranih jezika, značenje tih pojmova mora biti navedeno i na hrvatskom."*
Sve preuzete slike (2.1, 3.1, 3.2, 3.4, 3.5) i dalje su isključivo na engleskom.

Najjednostavnije rješenje bez ponovnog crtanja: proširiti naslov slike popisom prijevoda, npr.
*„Slika 3.2. Usporedba algoritama na okruženjima MuJoCo (engl. `Hopper-v1` — skakač, `Walker2d-v1`
— dvonožni hodač…). Preuzeto iz: [9]"*.

**Novo:** vlastite slike u poglavlju 6 su TensorBoard snimke sa sučeljem na engleskom
(`Environment/Catch`, `Smoothed`, `Value`, `Step`). To su nazivi metrika, ne prijevodni problem, ali
naslov slike treba dati hrvatsko značenje metrike — što je u većini naslova već učinjeno
(„Stopa hvatanja (Environment/Catch)"). Provedi to dosljedno i u onima gdje nije (npr. `Slika 6.7`).

### 4.3 Formule — i dalje nijedna nije numerirana

Rad sadrži ~30 jednadžbi bez brojeva. Uz to, u §5.5 su ostala **prazna mjesta gdje su jednadžbe
ispale iz teksta**: *„prema formuli ."*, *„dok pri  pada na svega 5 odluka"* i *„u sve tri testirane
konfiguracije (; ; )"*. Te tri rečenice treba popuniti (v. `VODIC_ZA_DOVRSETAK_RADA.md`, A14).

### 4.4 Veličine i jedinice

**Pogoršano u odnosu na prošlu reviziju.** Nova Tablica 5 (400k validacija) koristi **decimalne
točke kroz cijelu tablicu** (`1212.6`, `+21.9`, `−0.91`), dok Tablice 6 i 7 ispravno koriste zareze
(`1890,7`, `+1,45`). Uz to, u γ-tablici su ostale **markdown zvjezdice** (`**1,00**`) iz kopiranog
izvora. Stari propusti (`coef = 0.5` u §3.4.6, `99.4%` u Uvodu) i dalje stoje.

---

## 5. Jezik i stil

Upute: *„Rad treba pisati hrvatskim književnim jezikom u neodređenom ili trećem licu množine, a
ukoliko je to moguće, potrebno je pisati u pasivnim formama."*

**Poglavlje 2 i dalje krši ovo pravilo** na 8 mjesta („Recimo da…", „Formalno definirajmo…",
„Zamislimo scenarij…", „Označimo stanje…", „Možemo reći da…"). Poglavlja 4–6 pisana su ispravno.

**Miješanje jezika u naslovima** i dalje: „6.3.1 **Sparse arm** - emergentni progon" i
„6.3.2 **Shaped arm** – „shaping-farming"". Prijedlog: *„Rijetka nagrada — emergentni progon"* i
*„Oblikovana nagrada — oblikovno farmiranje i inverzija rezultata"*, uz engleski naziv u zagradi pri
prvom spominjanju.

**Novo — tipfeleri u naslovima slika poglavlja 6.** Uneseno je 17 novih naslova i u njima:
`Envirnoment` (4×), `Lenght` (2×), `Culmutive`, `Cumultive`, `Rezultait`, `objeruke` (2×),
`grafovasnimljenih`. Uz to u 15 naslova **nedostaje razmak iza broja** (`Slika 6.1Stopa…`), a
`Tablica 5400k validacija` se čita kao „Tablica 5400k". Sve je to vidljivo u Kazalu slika nakon `F9`.

**Nedosljedna kratica za engleski** — koristi se i `eng.` i `engl.`; Upute navode `engl.`

---

## 6. Literatura

Nepromijenjeno od prošle revizije. Rad koristi **IEEE stil** (inicijali ispred prezimena) za jedinice
[5]–[15], dok jedinice [1]–[4] **uopće nemaju autora**, a Upute za internetske izvore izričito traže:
*„uz čiju adresu svakako mora biti naveden autor citiranog materijala i datum"*.

| Jedinica | Problem | Prijedlog |
|---|---|---|
| [1] NVIDIA povijest | nema autora | dodati instituciju kao autora |
| [2] Lee Sedol / AlphaGo | nema autora | `Gomagic: "Lee Sedol and AlphaGo…", s Interneta, …` |
| [3] Dota 2 | nema autora, nema „s Interneta" | `OpenAI i dr.: "Dota 2 with Large Scale Deep RL", s Interneta, …` |
| [4] Lilian Weng | nema autora | `Weng, L.: "A (Long) Peek into Reinforcement Learning", …` |
| [5]–[15] | IEEE format | preurediti u `Prezime, I.: "Naslov", Izdavač, mjesto, godina.` |
| [13] ML-Agents dokumentacija | URL sadrži cijeli upit za pretraživanje (~200 znakova) | skratiti na osnovnu adresu |

---

## 7. Redoslijed izmjena

**A — obavezno prije predaje:**

1. Izbaciti Kafka/Taxi tekst iz Uvoda i popraviti „Error! Reference source not found." (odl. 113).
2. Napisati Zaključak, Sažetak, Ključne riječi, Title, Summary, Keywords.
3. Dodati Dodatak A (dokumentacija programske podrške) i premjestiti Sažetak ispred priloga.
4. Dodati naslov osmoj (γ) tablici i ukloniti markdown zvjezdice iz njezinih ćelija.

**B — mehaničke izmjene:**

5. `Ctrl+A` → `F9` — **sva tri kazala su zastarjela** (Kazalo slika prikazuje 9 starih stavki umjesto
   27, Kazalo tablica 6 od 8, Kazalo kodova i dalje „No table of figures entries found").
6. Prenumerirati tablice po poglavljima; premjestiti naslove Tablica 1–4 iznad tablica.
7. Ispraviti tipfelere i nedostajuće razmake u 15 naslova slika/tablica.
8. Ujednačiti decimalni zarez (Tablica 5 u cijelosti, `0.5`, `99.4 %`).
9. Numerirati formule po poglavljima; popuniti tri prazne jednadžbe u §5.5.
10. `Naslov2` postaviti na 12 pt; uključiti *Page break before* u stilu `Naslov1`.

**C — sadržajne dorade:**

11. Preurediti poglavlje 2 u neodređeno/treće lice množine (8 mjesta).
12. Prevesti strane pojmove na preuzetim slikama; dodati reference u naslove slika 3.1, 3.2, 3.4.
13. Prevesti naslove 6.3.1 i 6.3.2 na hrvatski.
14. Ujednačiti `eng.` / `engl.` kroz cijeli rad.
