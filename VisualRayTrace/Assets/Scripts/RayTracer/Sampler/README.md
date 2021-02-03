# Samplers

Verschiedene Funktionen des Raytracers benötigen ein Sampling Verfahren, um eine Menge an Punkten zu generieren. Diese Punkte entsprechen Koordinaten in einer Ebene, einem Raum oder einer Sphäre. In diesen Umgebungen sollen die Koordinaten einen möglichen großen Bereich abdecken und auf die gesamte Umgebung so verteilt sein, dass Ansammlungen von Koordinaten möglichst vermieden werden. Werden mehrere Punkte in unmittelbarer Nähe voneinander generiert, so wird der gleiche Bereich unnötig mehrfach repräsentiert, während andere Bereiche u.U. weniger bis gar keine Punkte zur Abbildung besitzen. Um eine möglichst genau Repräsentation der Umgebung zu erhalten müssen somit Verfahren eingesetzt werden, um diese Bedingungen zu erfüllen:

1. Punkte sind normalverteilt in einem Einheitsquadrat (Ebene) zur Minimierung von Klumpen und Abständen

2. Projektionen in die $x$- und $y-$Richtung sind ebenfalls normalverteilt

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

Die gemeinsame Abstraktion aller Sampling-Verfahren ist in der abstrakten Klasse `AbstractSampler` angesiedelt. Diese Klasse beinhaltet die elementaren Variablen, die für alle Sampling Verfahren benötigt werden. 

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

Die Variable `_numSamples` definiert die Anzahl an Punkten, die für ein Muster generiert werden. Unter einem Muster ist hier als das Ergebnis einer Iteration des Generierungsalgorithmus zu verstehen. Für Prozesse wie Anti-Aliasing ist eine Vielzahl an Koordinatensets notwendig um Artefakte zu vermeiden.  Jeder Pixel in unserem Viewport erhält ein eigenes Muster und eine individuelle Anzahl an generierten Punkten. Mit der Variable `_numSets` wird die Anzahl der Muster bzw. Iterationen des Generierungsalgorithmus festgelegt. 

Alle Punkte aller Iterationen werden musterübergreifend in der singulären Liste `_samples` gespeichert. Auf diese List wird dann mit einem externen Index zugegriffen, der aus `_count` (Muster Index/Offset) und `_jump` (Punkt Index/Offset) kalkuliert wird. Diese Trennung von Punktsammlung und Zugriffsindex erlauben den zufälligen Zugriff auf Punkte innerhalb eines Musters. Diese nicht-sequenzielle Abrufreihenfolge dient erneut dem Vermeiden von visuellen Artefakten.



```c#
// Viewport members
protected float _hStep; // Horizontale Schrittweite des Viewports
protected float _vStep; // Vertikale Schrittweite des Viewports
```

Abweichend von der Quellimplementierung [1] wird in dem Kontext unserer Unity Applikation nicht mit einer konstanten Schrittweite von $1$ gearbeitet. In der `C++` Applikation ist der Abstand zwischen zwei Pixeln (horizontal oder vertikal) stets konstant $1$. In der Unity-Szene jedoch wird diese Schrittweite dynamisch anhand des Viewport-Primitiv im Raum berechnet. Somit müssen diese Werte in Konstruktoren übergeben und in den Berechnungen berücksichtigt werden. In den Implementierungen der Unterklassen werden `_hStep` und `_vStep` dort eingesetzt, wo ursprünglich eine $1$ oder im Falle von Multiplikationen nichts stand ($1 \to$ neutrales Element).

### Methoden

```c#
public abstract void GenerateSamples();
```

Die abstrakte Methode definiert den Generierungsprozess der Punkte.  Sie wird von allen Unterklassen selbst implementiert. Die Verfahren der einzelnen Methoden werden in den folgenden Abschnitten genauer erläutert. Für die Implementierungen selbst sei hier auf den Quellcode der Unterklassen direkt verwiesen, da diese zum Großteil nur aus der eigenen Implementierung der abstrakten Methode bestehen.

```c#
public void MapSamplesToUnitDisk();
public void MapSamplesToHemisphere(float e);
```

ToDo: Projizierung in Kreis / Sphäre

```c#
public Vector2 SampleUnitSquare();
public Vector2 SampleUnitDisk();
public Vector3 SampleHemisphere();
```

Diese Zugriffsfunktionen liefern den nächsten Punkt im aktuellen Muster. Je nach notwendiger Projektionsbasis wird die entsprechende Funktion verwendet und synonym sind auch die Rückgabetypen dimensioniert. Nach Aufruf einer dieser Funktionen wird der lokale Punktindex `_count`im Muster inkrementiert. 

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
        <b>Abbildung 2.1: Verteilung eines RandomSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\RandomSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines RandomSampler mit n=256 [1]</b>
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

Im Verfahren des `JitteredSampler` wird durch Aufteilung des Pixelbereichs eine erste Verbesserung erzielt. Der Pixelbereich wird in $\sqrt{n} \times \sqrt{n}$  Teilbereiche aufgeteilt. In jedem Teilbereich wird ein Punkt generiert. Der Abstand des Punktes zum Mittelpunkt eines Teilbereich ist zufällig. Dieses Verfahren garantiert, das die Punkte auf die Teilbereiche verteilt werden und sich nicht innerhalb eines Teilbereichs zu Ansammlungen  vereinen können.

<figure>
    <img src="..\..\..\Resources\Samplers\Images\JitteredSampler01.png" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.1: Verteilung eines JitteredSampler Muster in x- und y-Richtung [1]</b>
    </figcaption>
</figure>
<figure>
    <img src="..\..\..\Resources\Samplers\Images\JitteredSampler02.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:left;">
        <b>Abbildung 2.2: Beispielmuster eines JitteredSampler mit n=256 [1]</b>
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







## Multi Jittered Sampler

## Hammersley Sampler



# Quellen

[1] [Ray Tracing from the Ground Up](http://www.raytracegroundup.com/), Kevin Suffern

[2] [Fisher-Yates Shuffle](https://stackoverflow.com/a/1262619), Stack Overflow