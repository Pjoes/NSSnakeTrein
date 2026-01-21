# NS Snake Game Design Document

## Brief

Selectieopdracht: Snake met een Trein in Unity

Doel:
Ontwikkel een innovatieve variant van het klassieke Snake-spel in Unity, waarin je een trein bestuurt in een 3D-omgeving met een top-down view die aanvoelt als een 2D-game. Deze opdracht test je vaardigheden in programmeren, game design, UI, en het toepassen van gameplay-mechanieken.

Opdrachtomschrijving:
Maak een spel waarin je een trein bestuurt met de toetsen W, A, S, D. De trein beweegt continu vooruit in een 3D-wereld, maar vanuit een vaste bovenaanzicht (top view) zodat het spelelement sterk lijkt op Snake.

Specificaties:

- Besturing: Je kunt de trein van richting veranderen met de toetsen W (vooruit), A (links), S (achteruit), D (rechts).

- Groeimechaniek: Telkens wanneer je een passagier (kan een simpele kubus zijn) oppakt, groeit de trein met één wagon erbij.

- Botsing: Als de trein zichzelf raakt (botsing met eigen wagons), is het spel afgelopen (game over).

- Power-ups: Implementeer twee verschillende power-ups naar eigen keuze.

- Enemies: Voeg minimaal één type vijand toe die de speler jaagt of volgt. Bedenk zelf wat er gebeurt als de vijand de trein raakt.

- Highscore: Toon continu de huidige score bovenaan het scherm, gebaseerd op het aantal opgepakte passagiers of overleefde tijd.

Aanlevering:

Een volledig werkend Unity-project met alle scripts, assets en instellingen.
Een korte video of tekstuele uitleg waarin je ingaat op je ontwerpkeuzes en eventuele uitdagingen.

## Look & Feel

Het lijkt mij leuk om een snake game te maken die geïnspireerd is door de look en feel van het typische *Wilde Westen* (Er is een model mee gestuurd, maar er staat dat deze gebruikt *kan* worden ;)). Dit zou betekenen dat de trein eerder zou lijken op de oude stoomlocomotief + kar combinatie in plaats van een moderne, elektrische trein.

![Eureka and Palisade Railroad](/docs/images/lookAndFeelTrein1.jpg)
![Virginia and Truckee Railroad](/docs/images/lookAndFeelTrein2.jpg)

Het spel zou dusdanig plaatsvinden in een omgeving als bijvoorbeeld de woestijn in New Mexico of Californië in de VS. Ik heb hiervoor gekozen, omdat mijn eerste idee van "vijanden" iets als bandieten of rovers was en dit niet zozeer zou passen in een moderne setting.  Het kleurenpalet zou voornamelijk geel en groen zijn, aangezien dit de kleuren zijn die je het meeste vindt in de woestijn.

## Gameplay

Aangezien de gameplay geïnspireerd moet zijn door de game *Snake*, is de gameplay al vrij bekend. De speler bestuurt een trein die begint met een enkele wagon, maar groeit naarmate er meer groepjes passagiers worden opgepakt. De trein kan in een bepaalde omgeving bewegen en bestuurd worden met WASD of de pijltjestoetsen. Daarnaast is het ook zo dat het oorspronkelijke spel altijd op een grid plaatsvindt (de slang kan alleen maar in exacte hoeken bewegen en draait altijd 90 graden), mij lijkt het me leuker om in deze opdracht de trein langzaam te laten draaien in plaats van in één keer een draai van 90 graden.

De trein stopt nooit met bewegen, tenzij deze een eigen wagon raakt of een ander obstakel. Dit betekent dat er niet altijd een knop ingedrukt moet blijven, maar de speler enkel de richting kan bepalen met WASD of de pijltjestoetsen. (W - omhoog, A - naar links, S - omlaag, D - naar rechts)

Daarnaast zullen er ook power-ups in de game komen om de ervaring van de speler wat leuker te maken. Hier ga ik verder op in. Hetzelfde geldt voor de vijanden. (De oorspronkelijke *Snake* heeft uiteraard geen vijanden en de speler moet alleen zijn eigen lichaam en de muren vermijden.) 

In het originele spel wordt de score bepaald door het aantal objecten dat de speler heeft opgegeten en dus de uiteindelijke lengte en in mijn versie zal dit hetzelfde blijven.

### Power-ups

Deze power-ups verschijnen maar een aantal seconden in de playing area, omdat de speler anders lang kan wachten om er eentje op te pakken. Daarnaast kan er ook maar één power-up tegelijkertijd aanwezig zijn op de map. Een aantal ideëen voor power-ups zijn:

- Noodrem: De trein beweegt een aantal seconden iets langzamer, wat het makkelijker maakt om deze te besturen.
- Centraal Station: Een "groot station" wat ervoor zorgt dat passagiers tijdelijk van iets verder opgepakt kunnen worden.
- Ontkoppelen: De trein ontkoppelt twee wagons om korter te worden, maar dit beïnvloedt de huidige score niet.
- Pantsertrein: De locomotief wordt versterkt en kan één keer een eigen wagon/muur raken zonder het spel te beëindigen. Dit beïnvloedt echter wel de score, om te zorgen dat de power-up niet te sterk is.

### Vijanden

Zoals eerder genoemd had ik al een aantal ideëen voor de look and feel van de vijanden en hier zal ik verder ingaan op de mechanics achter mogelijke vijanden:

- Bandieten te paard: Deze zouden kunnen bewegen zoals de spoken in Pac-Man. Ze zouden een berekening uitvoeren om te kijken waar de trein (mits er geen toets wordt ingedrukt) over een paar seconden zou zijn en daar vervolgens naartoe bewegen. Deze zouden despawnen zodra ze uit de playing area gaan.
- Treinrovers: Obstakels die zo nu en dan verschijnen en op de trein springen. De speler kan ze er echter wel afschudden door bijvoorbeeld snel te draaien of een ander item op te pakken. Doet de speler dit niet, dan verliezen ze een (aantal) wagon(s) en dus ook score.
- Cactusvelden: Dit zou de "muur" kunnen zijn. Als de speler een cactusveld raakt, hebben ze gelijk een game over. Deze zouden na een aantal seconden verdwijnen.

Aangezien het spel eindeloos is, zou het mogelijk zijn om vijanden vaker te laten spawnen naarmate de trein groeit, of de spawn rate hetzelfde te houden, aangezien de extra lengte van de trein op zich al het spel moeilijker maakt. Dit zou getest moeten worden.

### Score

De score wordt, in het algemeen, bijgehouden door het aantal passagiers dat de speler heeft opgepakt en dus, in het algemeen, door de lengte van de trein bepaald. Dit kan door zowel vijanden als power-ups beïnvloed worden, dus deze indicators zijn niet 100% accuraat.

Zodra de speler een game over heeft, wordt de score geplaatst in een bestand, zodat de highscore altijd terug kan worden gevonden (of bijvoorbeeld de top 3 scores). Dit geeft ook weer dat *arcade gevoel* dat je bij dit soort simpele games hebt.

## Art style

Om te zorgen dat het spel goed speelbaar blijft en qua performance ook stabiel blijft, kies ik voor simpele, low-poly models en niet te veel visuals tijdens de gameplay zelf.

## Technische beschrijving

Het spel zal in de huidige laatste beschikbare LTS versie van Unity gemaakt worden, welke op dit moment Unity 6000.0.60f1 is. Het spel zal speelbaar zijn op Windows computers.