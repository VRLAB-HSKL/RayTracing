# Lichtquellen

## Beleuchtung und Reflektion

Eine virtuelle Szene besteht zunächst nur aus geometrischen Objekten, definiert durch ihre Form und Transformation innerhalb der Szene. Diese Objekte werden durch einen Raytracer auf einen Viewport projiziert. Hierbei wird für jeden Pixel des Viewports die finale Farbe berechnet. Ein fundamentaler Bestandteil dieser Berechnung ist die Existenz von Lichtquellen. Diese Lichtquellen beleuchten geometrische Objekte und erzeugen einen Farbton anhand des Materials (Farbe, Beschaffenheit, Reflektion, ...) des Objektes.  

Die Beleuchtung einer Szene setzt sich somit aus den vorhandenen Lichtquellen zusammen. Ein Objekt wird von einer Lichtquelle entweder direkt oder indirekt beleuchtet. Eine direkte Erleuchtung besteht, wenn nach dem Austreten aus der Lichtquelle der erste weitere Kollisionspunkt das geometrische Objekt ist. Liegt zwischen der Lichtquelle und dem Kollisionspunkt mit dem geometrischen Objekt ein oder mehrere Kollisionspunkt/e (bspw. durch Kollision mit einem anderen Objekt mit reflektierendem Material), so spricht man von indirekter Beleuchtung. In diesem Kontext wird auch von lokaler (direkt) und globaler (indirekt) Beleuchtung gesprochen.



<figure>
    <img src="..\..\..\Resources\Lights\Images\DirectIllumination.jpg" style="width:50%">
    <figcaption style="padding:5px; text-align:center;">
        <b>Abbildung 1.1: Direkte und indirekte Beleuchtung</b>
    </figcaption>
</figure>



Der Weg eines Lichtstrahls kann durch auftretende Reflektionen schnell an Komplexität gewinnen. Wandert ein Lichtstrahl bspw. auf eine reflektierende Oberfläche, die parallel zu einer weiteren reflektierenden Oberfläche plaziert ist, so kann je nach Eintrittswinkel eine hohe Anzahl an Reflektionen auftreten. Oft wird für solche Fälle ein Maximalwert verwendet, um Reflektionen ab einer bestimmten Anzahl zu ignorieren. Die Reflektion eines Lichtstrahls auf einer Oberfläche hängt von dessen Beschaffung bzw. dem Material ab. Zur Modellierung des Reflektionsverhaltens eines Materials wird in der Computergrafik eine Kombination aus Spiegel- und diffuser Reflektion eingesetzt.

Bei einer perfekten Spiegelreflektion (ohne diffusen Anteil) wird jeder eingehender Lichtstrahl synonym zu seinem Eintrittswinkel zu der aufstehenden Normale im Kollisionspunkt in den singulären Ausgangsvektor $r$ komplett reflektiert. Diese Gewichtung kommt bei der Simulation von Spiegel- und transparente Materialien. Im anderen Extrem, also einer komplett diffusen Reflektion (ohne Spiegelanteil) wird der eintretende Lichtstrahl am Kollisionspunkt in alle möglichen Richtungen verteilt. Dies ist extrem rechenintensiv.



<figure>
    <img src="..\..\..\Resources\Lights\Images\PerfectSpecularReflection.jpg" style="width:50%">
    <figcaption style="padding:5px; text-align:center;">
        <b>Abbildung 1.2: Perfekte Spiegelreflektion</b>
    </figcaption>
</figure>

Zwischen diesen beiden Extremen existiert ein Medium. Bei dieser glänzenden Spiegelreflektion wird die Richtung des Ausgangsvektors ähnlich der perfekten Spiegelreflektion bestimmt. Statt einem singulären Ausgangsvektor wird eine weitere Anzahl von Austrittsvektoren um $r$ berechnet, um den Lichtstrahlvektor teilweise zu streuen. Ähnlich dem Maximalwert an Reflektionen, die bei dem Weg des Lichtstrahls beachtet werden, existiert ein Wert für die Anzahl an Streuvektoren, die zusätzlich zum singulären Ausgangsvektor $r$ ausgestrahlt werden. 



## AbstractLight

### Variablen

```c#
public bool CastShadows { get; set; } = true
```

Inhärent in allen Lichtquellen ist die Fähigkeit, Schatten zu werfen. Schatten bereichern die Szene und liefern qualitative Informationen über die vorhandenen Objekte. Ist diese Funktion aktiviert, so werden Bereiche, die von dieser Lichtquelle nicht beleuchtet werden, entsprechend schattiert dargestellt bzw. führen in der Farbberechnung zu einer Verdunklung der finalen Pixelfarbe. 

Bei einer Deaktivierung der Funktionalität werden Schatten, die von dieser Lichtquelle generiert werden, in die finale Farbberechnung nicht mit einbezogen. Dies führt zu einer Beschleunigung des Berechnungsvorgangs, da nicht überprüft werden muss, ob ein getroffener Punkt die Lichtquelle sieht. 

### Methoden

```C#
public abstract Vector3 GetDirection(RaycastHit hit);
public abstract Color L(RaycastHit hit);
public abstract bool InShadow(Ray ray, RaycastHit hit);
```

Die Methode `GetDirection()` liefert den Richtungsvektor zwischen der Lichtquelle und dem Kollisionspunkt. Der Kollisionspunkt ist im `RaycastHit` Parameter abrufbar. 



Die Methode `L()` liefer



Die Methode `InShadow()`



## AmbientLight

Eine `AmbientLight` Lichtquelle liefert eine konstante Umgebungsbeleuchtung für die gesamte Szene. Hiermit erhalten auch Objekte, die keine direkte Beleuchtung aus anderen Lichtquellen erhalten eine gewisse Grundbeleuchtung in der Szene, damit diese nicht komplett schwarz erscheinen. Stattdessen werden diese Bereiche mit den konstanten Farben der Objekte dargestellt. 

```C#
public float _ls;
public Color LightColor;
```

Der Strahlungsskalierungsfaktor `_ls` $\in [0, \infty)$  steuert die Intensität in der das Licht strahlt. In Kombination mit der `LightColor` Variable, die die Farbe des Lichts definiert, kann somit die Helligkeit des Lichts gesteuert werden.  

Die `GetDirection(...)` Methode liefert den Nullvektor zurück, da ein Umgebungslicht keinen konkreten Richtungsvektor hat, in der das Licht strahlt. Stattdessen ist das Licht dieser Quelle in der gesamtem Szene überall konstant vorhanden. 

`InShadow(...)` liefert immer den Wert `false` zurück. Die Umgebungslichtquelle wirft keinen Schatten.

Die Funktion `L(...)` kalkuliert die konstante Farbe der Umgebungslichtquelle. Die Farbe der Lichtquelle wird mit dem Skalierungsfaktor multipliziert, um die Farbe entsprechend abzuschwächen / zu verstärken. 

 ```c#
public override Color L(RaycastHit hit)
{
	return _ls * LightColor;
}
 ```



## DirectionalLight

Die Klasse `DirectionalLight` bildet ein Richtungslicht ab. Eine direktionale Lichtquelle sendet parallele Lichtstrahlen in eine gegebene Richtung, ähnlich der Sonne. Zwar handelt es sich bei der Sonne um eine Sphäre, jedoch sind die Strahlen die auf die Erde auftreffen quasi parallel. Da die Richtungslichtquelle jedoch stets mathematisch korrekt parallele Strahlen ausstrahlt, handelt es sich hierbei um eine reine mathematische Abstraktion.  

```c#
private float _ls;
private Color _color;
private Vector3 _dir;
private Vector3 _location;
```

Synonym zu einem `AmbientLight` Objekt besitzt die Klasse einen Strahlungsskalierungsfaktor `_ls` und eine Lichtfarbe `_color`. Weiterführend wurde ein `Vector3` Objekt hinzugefügt, um den Richtungsvektor der Lichtquelle abzubilden. Dieser Richtungsvektor zeigt die Richtung, aus der das Licht strahlt. 

Die `GetDirection(...)` Methode liefert diesen Richtungsvektor `_dir` zurück.

Mit der `InShadow(...)`Methode ermittelt mithilfe seiner Parameter ob der getroffene Zielpunkt im Schatten dieser Lichtquelle liegt. Der Punkt gilt als im Schatten dieser Lichtquelle, wenn die Distanz zwischen Treffpunkt und Lichtquelle größer, als die zurückgelegte Strecke des initialen Strahls. 

```c#
public override bool InShadow(Ray ray, RaycastHit hit)
{
    ...
    float t = tmpHit.distance;
    float d = Vector3.Distance(_location, ray.origin);
    return t < d;
    ...
}
```

Die Farbbestimmung mit der Methode `L(...)` erfolgt erneut durch die Multiplikation der Lichtfarbe mit dem Skalierungsfaktor 



## PointLight

Punktlichtquellen strahlen Licht aus einem Punkt in alle Richtungen aus. Es handelt sich erneut um eine reine mathematische Abstraktion. Je weiter ein Lichtstrahl von der Lichtquelle entfernt liegt, desto schwächer ist die Strahlung  die von dieser Lichtquelle ausgeht. Der Radius eines Umgebungskreises um die Lichtquelle ist somit invers proportional zu der Strahlungsstärke. Hierbei ist die Problematik, eine Intensität der Lichtquelle zu finden, die entfernte Objekte genug ausleuchtet, ohne das im Bereich nahe der Lichtquelle zu viel ausgestrahlt wird.



<figure>
    <img src="..\..\..\Resources\Lights\Images\PointLight.jpg" style="width:25%">
    <figcaption style="padding:5px; text-align:center;">
        <b>Abbildung 1.2: Punktlichtquelle mit zwei Kreisen mit unterschiedlichem Radius</b>
    </figcaption>
</figure>



```c#
private float _ls;
private Color _color;
private Vector3 _location;
```

Erneut existiert ein Skalierungsfaktor `_ls` und eine Lichtfarbe `_color`. Auch die Position der Lichtquelle ist in `_location` erneut festgehalten, um die Distanz zur Lichtquelle zu messen. Da diese Lichtquelle in alle Richtungen strahlt, existiert hier auch kein konkreter Richtungsvektor.

Von den Methoden ist hier `GetDirection(...)` hervorzuheben. Der Richtungsvektor wird berechnet durch die Differenz aus dem Lichtquellenpositionsvektor und den Treffpunkt des Strahls. Im nächsten Schritt wird der Vektor durch die Länge des Vektors dividiert, um einen Einheitsvektor zu generieren.

```c#
public override Vector3 GetDirection(RaycastHit hit)
{
    Vector3 tmp = (_location - hit.point);

    float length = Mathf.Sqrt(tmp.x * tmp.x + tmp.y * tmp.y + tmp.z * tmp.z);
    tmp.x /= length; 
    tmp.y /= length; 
    tmp.z /= length;

    return tmp; 
}
```

Die Methoden `InShadow(...)` und `L(...)` bestehen aus der gleichen Implementierung wie in der Klasse `DirectionalLight`



## AmbientOccluder

Die Klasse `AmbientLight` fügte unserer Szene eine konstante Beleuchtung hinzu. Hiermit wurden von anderen Lichtquellen nicht beleuchtete Objekte nicht mehr konstant schwarz dargestellt, sondern konstant mit der Farbe des Objekts (bzw. des Materials der Objektoberfläche). Eine Ansatz der näher der Realität ist wäre die Menge der umgebungsbasierten Beleuchtung die ein Punkt erhält nicht szenenglobal konstant zu verteilen, sondern davon abhängig zu machen, wie stark der Punkt von anderen Objekten blockiert wird. Um dies zu modellieren, wird eine 3D Hemisphäre über dem Punkt generiert (Sampling) und überprüft ob diese von einem anderen Objekt geschnitten wird. Je mehr die Hemisphäre von anderen Objekten blockiert wird, desto weniger umgebungsbasierte Beleuchtung erhält der Punkt.

Die Klasse `AmbientOccluder` beinhaltet erneut einen Skalierungsfaktor und eine Lichtfarbe

```c#
public float RadianceFactor;
public Color LightColor;
```

Hinzu kommen $3$ Vektoren. Sie werden innerhalb der `L(...)` Funktion initialisiert und zur Berechnung der Farbe verwendet. 

```c#
private Vector3 _u;
private Vector3 _v;
private Vector3 _w;
```

Zur Generierung und Abtastung der Hemisphäre wird ein Sampling Verfahren benötigt. Dieses generiert Punkte in einem Einheitsquadrat und bildet diese dann in eine 3D Heimsphäre ab. Aktuell wird ein `MultiJitteredSampler` verwendet

```c#
private AbstractSampler _sampler;
```

Die Farbe `_minAmount` wird zurückgegeben, wenn ein getroffener Punkt von einem Objekt okkludiert wird. 

```c#
private Color _minAmount;
```





## Bemerkungen



Members not in base class:

- for area and environmental lights  ls is in the emissive material that they use (chapter 18)
- the color data member is not in the base class because area lights can have a textured materials, where the color varies with position