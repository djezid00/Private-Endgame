# 6. Zaključak

U ovom radu implementirano je dvotimsko okruženje igre lovice unutar Unity ML-Agents okvira [6], u
kojem agenti Lovac i Bjegunac uče algoritmom MA-POCA [5] kroz samostalnu igru. Ispravnost
implementacije potvrđena je izravnim dokazom prisutnosti gubitka bazne linije (`BaselineLoss`),
karakterističnog isključivo za MA-POCA dodjelu zasluga, čime je implementacija razlučena od
neovisnog PPO učenja [7].

Proveden je kontrolirani eksperiment koji uspoređuje rijetku, isključivo terminalnu nagradu s
gustom nagradom oblikovanom potencijalnom funkcijom (PBS) [8], na kratkom (400 000 koraka) i dugom
(5 000 000 koraka) horizontu treniranja. Rezultati pokazuju da rangiranje dobiveno na kratkom
horizontu — gdje gusta nagrada daje približno tri puta veću kompetitivnu razliku — nije pouzdano:
na dugom horizontu rijetka nagrada dovodi do odlučujuće, dosljedne pobjede Lovca u sva tri testirana
sjemena, dok gusta nagrada dovodi do dosljednog poraza uzrokovanog lokalnim optimumom
(iskorištavanjem nagrade za blizinu bez dovršavanja hvatanja). Time je odgovoreno na istraživačko pitanje rada:
rijetka, terminalna nagrada dovoljna je za pojavu ponašanja potjere pod MA-POCA algoritmom i
samostalnom igrom, a gusto oblikovanje nagrade u ovom scenariju nije nužno te može biti štetno na
duljem horizontu treniranja.

Glavni doprinos rada je empirijski dokumentiran primjer u kojem teorijsko jamstvo invarijantnosti
optimalne politike kod potencijalnog oblikovanja nagrade [8] ne osigurava da će učenik tu politiku
doista dosegnuti pod diskontiranjem `γ < 1` i nestacionarnošću samostalne igre s više agenata, te
metodološka implikacija da kratkoročna validacija dizajnerskih odluka u MARL sustavima može dati
zavaravajuće, pa i suprotne zaključke u odnosu na dugoročan rezultat.

Budući rad uključuje dovršavanje pune agregacije rezultata preko sjemena (srednja vrijednost i
standardna devijacija stope hvatanja i duljine epizode, s pripadajućim grafovima pogrešaka),
provedbu usporedbenog eksperimenta s neovisnim PPO učenjem [7] radi izravnog opravdanja odabira
MA-POCA algoritma, te ispitivanje ponašanja pri različitim vrijednostima koeficijenta PBS
oblikovanja kako bi se utvrdilo je li uočeni lokalni optimum posljedica specifične jačine
oblikovanja ili se javlja u širem rasponu vrijednosti.
