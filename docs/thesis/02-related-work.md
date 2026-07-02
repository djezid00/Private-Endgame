# 2. Pregled srodnih radova

## 2.1 Podržano učenje i samostalna igra u igrama

Podržano učenje (engl. *reinforcement learning*, RL) temelji se na interakciji agenta s okruženjem
kroz pokušaje i pogreške, uz signal nagrade kao jedinu povratnu informaciju o kvaliteti djelovanja
[9]. Samostalna igra (engl. *self-play*), u kojoj agent trenira protiv prethodnih inačica vlastite
politike, pokazala se učinkovitom metodom za razvoj kompetitivnih strategija bez potrebe za
ljudskim demonstracijama. AlphaGo [2] i njegov nasljednik AlphaZero [10] pokazali su nadljudsku
razinu igre u igri Go koristeći samostalnu igru u kombinaciji s pretragom stabla. OpenAI Five [3]
proširio je pristup na kompleksniju, timsku igru Dota 2 s pet agenata po timu, dok je AlphaStar
[11] primijenio sličan pristup na igru StarCraft II uz djelomično opažljivo stanje.

## 2.2 Višeagentno podržano učenje i dodjela zasluga

Višeagentno podržano učenje (MARL) uvodi problem koji ne postoji kod pojedinačnog agenta: dodjelu
zasluga (engl. *credit assignment*) unutar tima kada je dostupna samo zajednička, timska nagrada.
COMA (*Counterfactual Multi-Agent Policy Gradients*) [12] rješava ovaj problem kontrafaktualnom
baznom linijom koja procjenjuje doprinos pojedinačnog agenta usporedbom stvarne akcije s
hipotetskom zamjenskom akcijom. QMIX [13] koristi monotonu mrežu miješanja (engl. *mixing network*)
koja timsku Q-vrijednost rastavlja na pojedinačne doprinose agenata, uz ograničenje da poredak
akcija po korisnosti mora biti očuvan (engl. *Individual-Global-Max* svojstvo).

MA-POCA (*Multi-Agent POsthumous Credit Assignment*) [5], implementiran unutar Unity ML-Agents
paketa [6], nadograđuje se na ideju kontrafaktualne bazne linije iz COMA pristupa [12], uz dodatnu
sposobnost ispravne dodjele zasluga agentima koji napuste epizodu prije njezinog završetka —
situacija koja se javlja u okruženjima s promjenjivim brojem agenata po timu tijekom epizode. Za
razliku od neovisnog PPO učenja [7], gdje svaki agent optimizira isključivo vlastitu nagradu bez
uvida u stanje ili doprinos ostalih članova tima, MA-POCA koristi centraliziranog kritičara tijekom
treniranja, dok se pri izvođenju svaki agent i dalje oslanja isključivo na vlastita opažanja
(paradigma *centralized training with decentralized execution*).

## 2.3 Oblikovanje nagrade

Kada je signal nagrade rijedak, proces učenja može biti izrazito spor ili se ne dogoditi unutar
razumnog broja koraka treniranja. Ng, Harada i Russell [8] formalizirali su potencijalno oblikovanje
nagrade (engl. *potential-based reward shaping*, PBS) i dokazali da dodavanje nagrade oblika
`F(s,s′) = γΦ(s′) − Φ(s)`, gdje je `Φ` proizvoljna potencijalna funkcija nad stanjima, ne mijenja
skup optimalnih politika podležećeg Markovljevog procesa odlučivanja (MDP). Ovo teorijsko jamstvo
odnosi se na jednog agenta u stacionarnom okruženju; njegova primjenjivost u nestacionarnim,
samostalno-igranim višeagentnim sustavima — gdje se okruženje (u obliku protivničke politike) mijenja
tijekom treniranja — u literaturi je manje istražena, a upravo je ta praznina motivacija za
eksperimentalni dio ovog rada (poglavlje 3).

Kao alternativa gustom oblikovanju nagrade, istraživan je i pristup temeljen na znatiželji (engl.
*curiosity-driven exploration*) [14], koji agenta nagrađuje za posjećivanje novih ili teško
predvidljivih stanja neovisno o vanjskoj nagradi zadatka. Iako koncepcijski različit od PBS
pristupa, dijeli isti temeljni cilj — ubrzati učenje pod rijetkom vanjskom nagradom.

## 2.4 Problem potjere i bijega u višeagentnim sustavima

Igre lovice i srodni scenariji potjere i bijega (engl. *pursuit-evasion*) [4] dugo su korišteni kao
testni zadaci za proučavanje emergentnog ponašanja u višeagentnim sustavima. OpenAI-jev rad o
emergentnom korištenju alata u igri skrivača (engl. *hide-and-seek*) [15] pokazao je da natjecateljski
pritisak između dva tima, uz dovoljno dugo treniranje samostalnom igrom, dovodi do niza sve
sofisticiranijih strategija bez eksplicitnog oblikovanja nagrade za svaku pojedinu strategiju.
DeepMind-ov rad o timskoj igri "capture the flag" [16] slično je pokazao razvoj koordiniranog
timskog ponašanja iz isključivo timske, terminalne nagrade. Ovi radovi podupiru pretpostavku da
rijetka, terminalna nagrada u kombinaciji sa samostalnom igrom može biti dovoljna za pojavu
kompleksnog ponašanja — pretpostavku koja se izravno ispituje u ovom radu na jednostavnijem,
kinematički simetričnom scenariju potjere i bijega.
