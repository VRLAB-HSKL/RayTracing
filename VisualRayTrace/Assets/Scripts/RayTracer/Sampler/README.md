# Samplers

Verschiedene Funktionen des Raytracers benötigen ein Sampling Verfahren, um eine Menge an Punkten zu generieren.  Es wird eingesetzt für _Ambient Occlusion_, Schattierung für `AreaLight` Objekte, Spiegelreflektionen, etc. Punkte entsprechen Koordinaten in einer Ebene, einem Raum oder einer Sphäre. In diesen Umgebungen sollen die Koordinaten einen möglichen großen Bereich abdecken und auf die gesamte Umgebung so verteilt sein, dass Ansammlungen von Koordinaten möglichst vermieden werden. Werden mehrere Punkte in unmittelbarer Nähe voneinander generiert, so wird der gleiche Bereich unnötig mehrfach repräsentiert, während andere Bereiche u.U. weniger bis gar keine Punkte zur Abbildung besitzen. Um eine möglichst genaue Repräsentation der Umgebung zu erhalten müssen somit Verfahren eingesetzt werden, um diese Bedingungen zu erfüllen:

1. Punkte sind normalverteilt in einem Einheitsquadrat (Ebene) zur Minimierung von Klumpen und Abständen

2. Projizierungen in die $x$- und $y-$Richtung sind ebenfalls normalverteilt

3. Minimalabstand zwischen Punkten

Alle Sampling Verfahren führen bei entsprechender Punktanzahl zu ähnlichen Ergebnissen. Die Frage ist immer, welche Verfahren, bieten mit welcher Anzahl an generierten Punkten, unter welchen Umständen zu effizient berechenbaren und visuell zufriedenstellenden Darstellungen.  

Basierend auf der Lektüre [1] wurde eine Hierarchie für Sampling-Verfahren als Strategie Pattern implementiert. Dies erlaubt die einfache Konstruktion weiterer Verfahren und dem effizienten Austauschen der aktuell verwendeten Strategie. Im Folgenden werden die Architektur und die bisher implementierten Sampling-Verfahren genauer vorgestellt.



<figure>
    <img src="..\..\..\Resources\Samplers\Images\SamplerUML.png" style="width:100%">
    <figcaption style="padding:5px; text-align:center;">
        <b>Abbildung 1: Klassendiagramm Sampler Architektur</b>
    </figcaption>
</figure>



## Abstract Sampler

Die gemeinsame Abstraktion aller Sampling-Verfahren ist in der abstrakten Klasse `AbstractSampler` angesiedelt. Diese Klasse beinhaltet die elementaren Variablen und Methoden, die von allen Sampling Verfahren verwendet werden. 

### Variablen

```c#
// Sampling
protected int _numSamples; // Anzahl der Punkte im Muster
protected int _numSets; // Anzahl der Muster
protected List<Vector2> _samples; // Liste der generierten Punkte
protected List<int> _shuffeledIndices; // Gemischte Indexe
protected int _count; // Aktuelle Anzahl verwendeter Punkte
protected int _jump; // Zufälliger Sprungabstand (Offset)

// Projection
protected List<Vector2> _diskSamples; // Projizierte Punkte (Kreis)
protected List<Vector3> _hemisphereSamples; // Projizierte Punkte (Spähre)
```

Der Bereich Sampling beinhaltet alle Klassenvariablen des Generierungsprozess. Alle Variablen haben die Zugriffsstufe `protected`, da die Unterklassen in ihren Algorithmen diese verwenden. 

Die Variable `_numSamples` definiert die Anzahl an Punkten, die für ein Muster generiert werden. Unter einem Muster ist hier als das Ergebnis einer Iteration des Generierungsalgorithmus zu verstehen. Für Prozesse wie Anti-Aliasing ist eine Vielzahl an Koordinatensets notwendig um Artefakte zu vermeiden. Mit der Variable `_numSets` wird die Anzahl der Muster bzw. Iterationen des Generierungsalgorithmus festgelegt. 

Alle Punkte aller Iterationen werden musterübergreifend in der singulären Liste `_samples` gespeichert. Auf diese Liste wird dann mit einem externen Index zugegriffen, der aus `_count` (Muster Index/Offset) und `_jump` (Punkt Index/Offset) kalkuliert wird. Um also auf den 5. Punkt von 10 im 3. Set zuzugreifen, wäre der Zugriffindex entsprechend `_samples[24]`.  



<figure>
    <img src="..\..\..\Resources\Samplers\Images\SetIndexing.png" style="width:50%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.1: Indizierung der eindimensionalen Liste _samples</b>
    </figcaption>
</figure>

Diese Trennung von Punktsammlung und Zugriffsindex erlauben den direkten Zugriff auf Punkte innerhalb eines Musters. Jedoch darf dieser Zugriff auch musterübergreifend nicht sequenziell erfolgen, da sonst erneut spalten- oder zeilenweise Artefakte entstehen. Somit wird zusätzlich ein zufallsbasierter Wert auf den berechneten Index addiert, um auch in dieser Dimension in zufälliger Reihenfolge Punkte abzurufen. 

```c#
// Viewport members
protected float _hStep; // Horizontale Schrittweite des Viewports
protected float _vStep; // Vertikale Schrittweite des Viewports
```

Abweichend von der Quellimplementierung [1] wird im Kontext unserer Unity Applikation nicht mit einer konstanten Schrittweite von $1$ gearbeitet. In der `C++` Applikation ist der Abstand zwischen zwei Pixeln (horizontal oder vertikal) stets konstant $1$. In der Unity-Szene jedoch wird diese Schrittweite dynamisch anhand des Viewport-Primitiv im Raum berechnet. Somit müssen diese Werte in Konstruktoren übergeben und in den Berechnungen berücksichtigt werden. In den Implementierungen der Unterklassen werden `_hStep` und `_vStep` dort eingesetzt, wo ursprünglich eine $1$ oder im Falle von Multiplikationen nichts stand ($1 \to$ neutrales Element).

### Methoden

```c#
public abstract void GenerateSamples();
```

Die abstrakte Methode definiert den Generierungsprozess der Punkte.  Sie wird von allen Unterklassen selbst implementiert. Die Verfahren der einzelnen Methoden werden in den folgenden Abschnitten genauer erläutert. 

```c#
public void MapSamplesToUnitDisk();
```

Die Methode `MapSamplesToUnitDisk()` projiziert die Punkte im Einheitsquadrat in einen Einheitskries. Hierbei sollte die Qualität der Verteilung möglichst erhalten bleiben. Shirley bietet hierfür das konzentrische Mapping von Einheitsquadrat nach Einheitskreis. Zwei Objekte gelten als konzentrisch, wenn sie das gleiche Zentrum besitzen. Dies ist bei unseren Einheitsobjekten der Fall. Sie werden beide in Quadranten aufgeteilt und Punkte werden in die gleichen Quadranten abgebildet. Die projizierten Punkte werden in der Liste `_diskSamples` gespeichert.   



<figure>
    <img src="..\..\..\Resources\Samplers\Images\concentricMapping.png" style="width:50%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Visualisierung des konzentrischen Mapping [1]</b>
    </figcaption>
</figure>



```c#
public void MapSamplesToHemisphere(float e);
```

Mit `MapSamplesToHemisphere(float e)` werden Punkte aus dem Einheitsquadrat in eine 3D Sphäre projiziert. Die Punkte sollen auf der Oberfläche der Sphäre so verteilt sein, dass die Dichte $d$ einer Kosinus-Verteilung entspricht. Hiermit ist gemeint, dass sie einer potenzierten Kosinus-Funktion auf den Polarwinkel $\theta$ der Sphäre entspricht. Der Polarwinkel wird im Bezug zur Polarachse gemessen, also der Normalen der Fläche auf die ein Strahl in unserem Raytracer trifft. 

$d = cos^e(\theta)$  

Die Potenz $e \in [0, \infty)$ wird der Funktion als `float` Parameter übergeben. Je höher der Wert für $e$ gewählt wird, desto rapider sinkt die Dichte der Verteilung. 

```c#
public Vector2 SampleUnitSquare();
public Vector2 SampleUnitDisk();
public Vector3 SampleHemisphere();
```

Diese Zugriffsfunktionen liefern den nächsten Punkt. Je nach notwendiger Projektionsbasis wird die entsprechende Funktion verwendet und synonym sind auch die Rückgabetypen dimensioniert. Nach Aufruf einer dieser Funktionen wird der lokale Punktindex `_count`im Muster inkrementiert. 

### Fisher-Yates Shuffle

In der Lektüre [1] wurde die gesamte Architektur mithilfe von `C++` implementiert. Die dort verwendete Standardbibliothek bietet mit `std::shuffle` eine allgemeine Funktion zur zufälligen Reorganisation eines Intervalls/Liste. In der aktuellen Version von `C#` existiert keine vergleichbare Schnittstelle. Um die Reihenfolge der Elemente einer Liste zufällig anordnen zu können, wurde eine individuelle Version des [Fisher-Yates Shuffle](https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle) implementiert. [2]  

```c#
private static List<int> Shuffle(List<int> list)
{
	int n = list.Count;
    while (n > 1)
    {
    	n--;
        int k = UnityEngine.Random.Range(0, n + 1);
        int value = list[k];
        list[k] = list[n];
        list[n] = value;
    }

    return list;
}
```



## Regular Sampler

Mit einem regulären Sampler werden keine abweichenden Punkte generiert. Alle Punkte entsprechen dem Mittelpunkt des Viewport-Pixels. Dieser Sampler dient der Simulation eines Ray-Tracing-Vorgangs ohne Einsatz von Sampling Verfahren, die versuchen die gesamte Fläche abzubilden. Ist Sampling deaktiviert, so wird dieser Sampler verwendet. 

Dies ist eine gute Gelegenheit den Grundaufbau der `GenerateSamples()` Funktion zu erläutern. Sie besteht aus mehreren `for`-Schleifen. Es werden `_numSets` Muster konstruiert. In jedem Muster wird ein Pixel vertikal (`p`) bzw. horizontal (`q`) durchlaufen. Für jeden Mittelpunkt eines Schritts innerhalb des Pixels werden Punkte erzeugt, in diesem Fall trivialerweise nur der Mittelpunkt selbst.

```c#
public override void GenerateSamples()
{
	int n = (int)Math.Sqrt((float)_numSamples);

    for (int j = 0; j < _numSets; j++)
    {
    	for (int p = 0; p < n; p++)
        {
        	for (int q = 0; q < n; q++)
            {
            	_samples.Add(new Vector2((q * _hStep) / (float)n, (p * _vStep) / (float)n));
            }
        }
	}
}
```



## Random Sampler

Ein `RandomSampler` generiert alle seine Punkte rein zufällig. Hierzu wird der Zufallszahlgenerator der `UnityEngine` verwendet. Da alle Punkte zufällig in beide Richtungen generiert werden, kann keine der Bedingungen eines guten Sampling Verfahrens garantiert werden. Leicht entstehen große Ansammlungen von Punkten und große Flächen werden nicht repräsentiert. 

<figure>
    <img src="..\..\..\Resources\Samplers\Images\RandomSampler01.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 3.1: Verteilung eines RandomSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\RandomSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 3.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
    </figcaption>
</figure>        




Die `GenerateSamples()` Funktion konstruiert erneut die definierte Anzahl an Mustern. In jedem Muster werden die Punkte zufällig im Pixelbereich generiert und der Liste hinzugefügt.

````c#
public override void GenerateSamples()
{
	for (int p = 0; p < _numSets; ++p)
    {
    	for (int i = 0; i < _numSamples; ++i)
        {
        	float hRnd = Random.Range(0f, _hStep - 1e-5f);
            float vRnd = Random.Range(0f, _vStep - 1e-5f);
            Vector2 sp = new Vector2(hRnd, vRnd);
            _samples.Add(sp);
    	}
	}
}
````



## Jittered Sampler

Im Verfahren des `JitteredSampler` wird durch Aufteilung des Pixelbereichs eine erste Verbesserung erzielt. Der Pixelbereich wird in $n \times n$ Teilbereiche (Schichten) mit $n \times n = $ `_numSamples` aufgeteilt, um `_numSamples`  Punkte zu generieren. Entsprechend sollte bei Einsatz von Verfahren dieser Art ein Wert für `_numSamples` gewählt werden, der zu einer ganzzahligen Wurzel führt (z.B. Zweierpotenzen) da diese Wurzel zur Dimensionierung verwendet wird. 

In jedem Teilbereich wird ein einziger Punkt generiert. Der Abstand des Punktes zum Mittelpunkt eines Teilbereich ist zufällig. Dieses Verfahren garantiert, das die Punkte auf die Teilbereiche verteilt werden und sich innerhalb eines Teilbereichs keine Ansammlungen bilden.



<figure>
    <img src="..\..\..\Resources\Samplers\Images\JitteredSampler01.png" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 4.1: Verteilung eines JitteredSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\JitteredSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 4.2: Beispielmuster eines JitteredSampler mit n=256 [1]</b>
    </figcaption>
</figure>        




ToDo: GenerateSamples() Erläuterung

```c#
public override void GenerateSamples()
{
 int n = (int)Math.Sqrt(_numSamples);
 for (int p = 0; p < _numSets; ++p)
 {
  for (int j = 0; j < n; ++j)
  {
   for (int k = 0; k < n; ++k)
   {
    float hRnd = UnityEngine.Random.Range(0f, _hStep - 1e-5f);
    float vRnd = UnityEngine.Random.Range(0f, _vStep - 1e-5f);                
				
    Vector2 sp = new Vector2(
     (k * _hStep + hRnd) / (float)n,
     (j * _vStep + vRnd) / (float)n
    );
                
    _samples.Add(sp);
   }
  }
 }
}
```





## N-Rooks Sampler

Eine weitere Verbesserung in den eindimensionalen Projizierungen kann mit einem `NRooksSampler` erreicht werden. Ähnlich dem vorherigen Verfahren wird der Bereich in `_numSamples` $\times$ `_numSamples` einheitliche Teilbereiche aufgeteilt. Es werden `_numSamples`$^2$ Punkte generiert. Zusätzlich wird jedem Punkt die Kondition auferlegt, dass seine $x$- und $y$-Koordinate im gesamten Muster einzigartig ist. Von dieser Kondition ist auch der Name des Verfahrens abgeleitet. Sieht man das Einheitsquadrat mit seinen Teilbereichen als Schachbrett und setzt man auf das Feld jeden Punktes eine Turm Schachfigur, so kann kein Turm einen anderen Turm schlagen. Entsprechend werden die Punkte mit Koordinaten an der Hauptdiagonale des Quadrats initialisiert. 

<figure>
    <img src="..\..\..\Resources\Samplers\Images\NRooksSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 5.1: Initialisierung der Punkte an der Hauptdiagonale (n=16) [1]</b>
    </figcaption>
</figure>

Im nächsten Schritt werden die kalkulierten $x$- und $y$-Koordinaten mit einem Algorithmus so vertauscht, dass die Punkte immer noch die Einzigartigkeitsbedingung erfüllen, jedoch zufällig verteilt sind. Dieser Vertauschungsprozess ist in den Methoden `ShuffleXCoordinates()` und `ShuffleYCoordinates()` implementiert. Sie generieren einen Zufallswert und vertauschen in ihrer Koordinatendimension mit einem anderen Punkt.

```c#
private void ShuffleXCoordinates();
private void ShuffleYCoordinates();
```



<figure>
    <img src="..\..\..\Resources\Samplers\Images\NRooksSampler03.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 5.2: Punkte nach der Neuordnung der Koordinaten (n=16) [1]</b>
    </figcaption>
</figure>  



Wir betrachten ein generiertes Muster. Die Verteilung weist in den einzelnen, singulären Dimensionen (1D) $x$ und $y$ im Vergleich zu einem `JitteredSampler` eine Verbesserung auf. Vergleicht man das komplette Muster (2D) jedoch mit einem `RandomSampler`, so ist keine signifikante Verbesserung der Verteilung ersichtlich. Ein `JitteredSampler` erreicht in 2D bessere Ergebnisse.

<figure>
    <img src="..\..\..\Resources\Samplers\Images\NRooksSampler01.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 5.3: Beispielmuster eines NRooksSampler (n=256) [1]</b>
    </figcaption>
</figure>


Die `GenerateSamples()` Methode erzeugt die Punkte entlang der Hauptdiagonale. Danach werden die Punkte in ihren $x$- und $y$- Koordinaten vertauscht.

```C#
public override void GenerateSamples()
{
 // Generate samples among main diagonal
 for (int p = 0; p < _numSets; ++p)
 {
  for (int j = 0; j < _numSamples; ++j)
  {
   float hRnd = Random.Range(0f, _hStep - 1e-5f);
   float vRnd = Random.Range(0f, _vStep - 1e-5f);
   float x = (j * _hStep + hRnd) / (float)_numSamples;
   float y = (j * _vStep + vRnd) / (float)_numSamples;

   _samples.Add(new Vector2(x, y));
  }
 }

 ShuffleXCoordinates();
 ShuffleYCoordinates();
}
```



## Multi Jittered Sampler

Um die Qualität der 2D Verteilung eines `NRooksSampler` zu verbessern, wurde 1994 von Kenneth Chiu et. al. [3] das Verfahren des `MultiJitteredSampler` vorgestellt. Diese Verfahren vereint die Vorgehensweisen von`JitteredSampler` und `NRooksSampler` Implementierungen. 

Zunächst wird das Quadrat in Teilbereiche unterteilt. Diese Teilbereiche werden nun lokal erneut in Unterbereiche aufgeteilt. In jeden oberen Teilbereich werden nun sequenziell Punkte erzeugt. Im ersten oberen Teilbereich wird der Punkt im ersten Unterbereich platziert. Im zweiten oberen Teilbereich wird ein Punkt im zweiten Unterbereich generiert. 

Hat dieser Algorithmus seine Aufgabe erledigt, so haben wir auf der oberen Schicht eine Verteilung eines `JitteredSampler` generiert. Zusätzlich haben wir eine Verteilung der Punkte erzielt, welche auf der oberen Schicht die horizontale und vertikale Einzigartigkeit der Koordinaten eines `NRooksSampler`erfüllt.  Vertauschen wir nun entsprechend die Koordinaten der Punkte wie bei _NRooks_, so ist auch bezogen auf die einzelnen unteren Teilbereiche die Konditionen eines `NRooksSampler` erfüllt.

Hierdurch wird sowohl eindimensional (`NRooksSampler`) als auch zweidimensional (`JitteredSampler`) gesehen eine gute bzw. sogar bessere 2D Verteilung erzielt. Die Implementierung von `GenerateSamples()` orientiert sich an den vorhandenen Funktionen der kombinierten Sampler Klassen.



<figure>
    <img src="..\..\..\Resources\Samplers\Images\MultiJitteredSampler01.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.1: Verteilung eines RandomSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\MultiJitteredSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\MultiJitteredSampler03.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
    </figcaption>
</figure>



## Hammersley Sampler

Das letzte Sampling-Verfahren wurde bereits den 1960er Jahren erstmals vorgestellt [4]. Nach einem der Autoren benannt, erzeugt dieses Verfahren Punkte in einem Einheitsquadrat mithilfe einer _radical inverse function_. Diese Funktion nimmt als Parameter einen Integer entgegen, und spiegelt dessen binäre Repräsentation $i$ am Dezimalpunkt. 

Wir können die binäre Repräsentation eines Integer als Summe von Multiplikationen von Koeffizienten $a_j \in \{0, 1\}$ und Zweierpotenzen $2^j$ darstellen:

$i_2 = \sum\limits_{j = 0}^n a_j (i) 2^j = a_0(i) 2^0 + a_1(i) 2^i + ... + a_{j-1}(i)2^{j-1}$

Die Zahl $12$ sieht bspw. in dieser Form so aus:

$12 = 1100_2 = 0 \cdot 2^0 + 0\cdot 2^1 + 1\cdot 2^2 + 1\cdot 2^3$

Auf diese Zahlen können wir nun die angesprochene _radical inverse function_ anwenden. Sie spiegelt die Zahl in ihrer binären Form am Dezimalpunkt.

$\Phi_2(i) = \sum\limits_{j = 0}^{n} a_j(i) 2^{-j-1} = a_0(i)\frac{1}{2} + a_1(i)\frac{1}{4} + ... + a_{j-1}(i) 2^{-(n-1)-1}$

Die Funktion auf die Zahl $12$ angewendet würde dieses Ergebnis liefern:

$\Phi_2(12) = \Phi(1100_2) = 0\cdot 2^{-1} + 0\cdot2^{-2} + 1\cdot2^{-3} + 1\cdot2^{-4} = \frac{1}{8} + \frac{1}{16} = \frac{3}{16} = 0.1875 = 0011_2$

Betrachten wir das Ergebnis, so sehen wir das sie dem horizontal gespiegelten Eingangsparameter entspricht ($1100 | 0011$). Mithilfe dieser Funktion werden Punkte mit einem `HammersleySampler` generiert. Dieses Verfahren setzt hier die folgende Formel zur Generation der Koordinaten ein:

$p_i = (x_i, y_i) = [1/n, \Phi_2(i)]$

Hierdurch werden sowohl in der $x$-, als auch in der $y$-Dimension Punkte mit gleichem Abstand generiert. In 1D und 2D führt dies zu einer sehr guten Verteilung der Punkte mit minimalem Abstand. Jedoch existiert durch die fest definierte mathematische Funktion die diesem Verfahren zugrunde liegt, für jeden Eingangsparameter nur ein einzigartiges Ergebnis. Durch solch konstante Randerscheinungen können stets visuelle Artefakte bzw. Aliasing entstehen.



<figure>
    <img src="..\..\..\Resources\Samplers\Images\HammersleySampler01.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.1: Verteilung eines RandomSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\HammersleySampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\HammersleySampler03.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
    </figcaption>
</figure>


## Bemerkungen

Die vorgestellten Sampling-Verfahren bieten eine breite Auswahl an Möglichkeiten, für unterschiedliche Anwendungen innerhalb der Unity Szene. In der folgenden Tabelle sind die vorhandenen Sampler Klassen aufgeführt:

|                        | 1D   | 2D   | Qualität | Inline Berechnung |
| ---------------------- | ---- | ---- | -------- | ----------------- |
| `RegularSampler`       | -    | -    | -        | ja                |
| `RandomSampler`        | ~    | ~    | ~        | ja                |
| `JitteredSampler`      | +    | +    | +        | ja                |
| `NRooksSampler`        | ++   | ~    | +        | nein              |
| `MultiJitteredSampler` | ++   | ++   | ++       | nein              |
| `HammersleySampler`    | ++   | ++   | +        | ja                |



Mit jedem neuen Verfahren wurde eine Verbesserung der Qualität, wenn auch teilweise nur eindimensional, erreicht. Mit einem `RegularSampler` wird ein Großteil der Umgebung im Umkreis des Zentralpunktes nicht abgebildet, da kein konkretes Sampling stattfindet. Ein `RandomSampler` bietet eine erste Verbesserung, in dem die nähere Umgebung der Zielkoordinate zufällig abgetastet wird. Das `JitteredSampler` Verfahren bildet einen ersten Ansatz mit vorhersehbarer Qualität und verbessert somit die durchschnittliche Verteilung in 1D und 2D. Implementiert man einen `NRooksSampler`, so kann zumindest im eindimensionalen Bereich das `JitteredSampler` Verfahren geschlagen werden.  Verbindet man die beiden Ansätze im Konstrukt des `MultiJitteredSampler`, so kann die beste Qualität in beiden Dimensionsprojizierungen generiert werden. Zuletzt existiert mit dem `HammersleySampler` ein Verfahren, das zwar eine hohe Qualität in der theoretischen Verteilung liefert, allerdings durch seinen harten Determinismus im Anti-Aliasing trotzdem zu visuellen Artefakten führt.

In der Lektüre wird für die Anzahl an Punkten die pro Muster generiert werden der Default Wert $83$ verwendet. Dieser Wert wurde übernommen, jedoch handelt es sich hierbei lediglich um eine angemessen große Primzahl und kann beliebig im Unity Inspector des Scripts verändert werden. 




# Quellen

[1] [Ray Tracing from the Ground Up](http://www.raytracegroundup.com/), Kevin Suffern

[2] [Fisher-Yates Shuffle](https://stackoverflow.com/a/1262619), Stack Overflow

[3] Graphics Gems, Chiu & Wang & Shirley (1994)

[4] [Monte Carlo Methods](http://www.cs.fsu.edu/~mascagni/Hammersley-Handscomb.pdf), Hammersley & Hanscomb (1964)