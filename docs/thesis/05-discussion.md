# 5. Rasprava

## 5.1 Odgovor na istraživačko pitanje

Rezultati (poglavlje 4) daju izravan odgovor na istraživačko pitanje postavljeno u uvodu: agent
Lovac razvija ponašanje potjere isključivo na temelju rijetke, terminalne nagrade, bez potrebe za
gustim oblikovanjem nagrade — pod uvjetom dovoljno dugog horizonta treniranja. Pri 5 000 000
koraka rijetki režim postiže odlučujuću prednost (ELO ≈1890 naspram ≈670) u sva tri testirana
sjemena, dok gusti (PBS) režim gubi u sva tri sjemena. Gusto oblikovanje nagrade u ovom je slučaju
ne samo nepotrebno nego i štetno na duljem horizontu.

## 5.2 Tumačenje preokreta rezultata između kratkog i dugog horizonta

Preokret rangiranja između 400 000 i 5 000 000 koraka (§4.3, §4.4) objašnjava se prirodom
lokalnog optimuma u koji upada gusti režim. Lovac u gustom režimu maksimizira potencijalnu nagradu
zadržavanjem blizine Bjegunca, umjesto dovršavanja hvatanja — ponašanje vidljivo iz kombinacije
visoke pojedinačne nagrade (koja uključuje PBS član) i timske nagrade blizu −1 (koja ga ne
uključuje). Teorijsko jamstvo potencijalnog oblikovanja nagrade [8] odnosi se na nepromjenjivost
*optimalne* politike, ne i na jamstvo da će učenik tu politiku doista dosegnuti. Dva su faktora
vjerojatno doprinijela ovom raskoraku u eksperimentu: faktor diskontiranja `γ = 0,99 < 1` unosi
malu, ali postojanu nagradu za *bivanje* blizu, ne samo za *približavanje*, čime slabi strogu
invarijantnost; a nestacionarnost samostalne igre s više agenata — gdje se protivnička politika
mijenja tijekom treniranja — dodatno odstupa od pretpostavki pod kojima je teorem izvorno dokazan za
jednog agenta u stacionarnom okruženju.

## 5.3 Implikacije na metodologiju validacije MARL sustava

Rezultat pri 400 000 koraka (§4.3) sam za sebe upućivao bi na suprotan zaključak: da gusto
oblikovanje nagrade poboljšava rezultat otprilike tri puta. Taj bi zaključak, izveden isključivo iz
kratkoročne validacije, bio pogrešan. Ovaj nalaz ima praktičnu implikaciju za validaciju MARL
sustava: kratkoročni rezultati mogu poslužiti za provjeru ispravnosti implementacije (poglavlje 3 i §4.1),
ali ne i za konačno rangiranje dizajnerskih odluka poput oblikovanja nagrade, osim ako
se ne potvrde na horizontu bliskom onome koji se stvarno koristi za konačne modele.

## 5.4 Ograničenja rada

Nekoliko ograničenja treba uzeti u obzir pri tumačenju rezultata. Prvo, rezultati na dugom
horizontu (§4.4) očitani su iz konzolnih zapisa treniranja po ponašanju; potpuna agregacija
podataka preko tri sjemena (srednja vrijednost i standardna devijacija za stopu hvatanja, duljinu
epizode i vrijeme do hvatanja, s pripadajućim grafovima) iz TensorBoard zapisa još je u izradi. Drugo, eksperiment je
proveden isključivo unutar jednog, pojednostavljenog scenarija igre lovice s kinematički
identičnim agentima; generalizacija zaključka na scenarije s asimetričnom kinematikom ili većim
brojem agenata po timu nije ispitana. Treće, usporedba s neovisnim PPO učenjem [7] — koja bi izravno
opravdala odabir MA-POCA algoritma umjesto jednostavnijeg pristupa — nije provedena u sklopu ovog
rada. Četvrto, testirana je samo jedna vrijednost koeficijenta PBS oblikovanja (`coef = 0,5`);
ponašanje pri manjim koeficijentima ostaje neispitano.
