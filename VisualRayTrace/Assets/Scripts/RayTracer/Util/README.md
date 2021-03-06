# RayTraceUtility

Der RayTracer Prozess bietet eine Vielzahl an Einstellungen, um das resultierende Bild, das auf dem Viewport generiert wird, zu beeinflussen. Um diese Werte global zugänglich zu machen, wurde die statische Klasse `RayTraceUtility` eingeführt. Somit kann an einer konkreten Stelle in der Codebasis der Wert angepasst werden. 

## Variablen

In unserer Szene existiert eine kleine, endliche Menge an möglichen Materialien. Zwar können diese Materialien unterschiedliche Farben annehmen, ihre Beschaffenheit bzw. wie sie auf einen eingehenden Strahl reagieren ist in allen Färbungen eines Materials gemein. Also bietet es sich an, statische Instanzen dieser `Material` Unterklassen anzulegen und nur die entsprechenden Eigenschaften (wie Farbe) dynamisch zu setzen. In Form des `Singleton` Entwurfsmuster wurden die 3 Materialien (`SolidColor`, `Metal`, `Dielectric`) implementiert. Zwar ist diese Implementierung nicht _thread-safe_, jedoch erfüllt sie ihren Zweck in der aktuellen Abrufreihenfolge.

```c#
public static ReflectiveMaterial MetalMaterial
{ 
    get
    {
        if(_metalMat is null)
        {
            ...
        }

        return _metalMat;
    }    
}
```

Zur Initialisierung der Metall (`ReflectiveMaterial`) und Glas (`DielectricMaterial`) Objekte werden Parameter benötigt (Koeffizienten, Farben, etc.). Diese Parameter wurden als statische Attribute der Klasse implementiert, mit entsprechenden Default-Werten aus der Lektüre [1]. Diese können entweder fest im Code selbst oder in der Applikation durch das Einstellungsmenü dynamisch gesetzt werden.  

```c#
// Metal parameters
public static float Metal_KA = 0.25f;
public static float Metal_KD = 0.5f;
public static float Metal_KS = 0.15f;
public static int Metal_EXP = 100;
public static float Metal_KR = 0.75f;

// Dielectric parameters
public static float Dielectric_KS = 0.2f;
public static float Dielectric_EXP = 100f;
public static float Dielectric_EtaIN = 1.5f;
public static float Dielectric_EtaOUT = 1f;
```



## Methoden

```c#
public static Color CreateNonHitColor(Vector3 direction);
public static MaterialType DetermineMaterialType(Material mat);
public static Color MaxToOne(Color c);
public static Color ClampToColor(Color rawColor)
```

Die Methode `CreateNonHitColor(...)` wird zur Generierung einer Farbe verwendet, für den Fall das der Strahl auf keine Oberfläche trifft. Mit `DetermineMaterialType(...)` wird das Material des getroffenen Objekts auf einen der 3 Materialtypen der Architektur abgebildet. `MaxToOne(...)` ist ein Operator zum Abbilden von sogenannten Out-Of-Gamut Farben in den Farbbereich der vom System abbildbaren Farben. Die Funktion `ClampToColor(...)` überprüft die übergeben Farbe. Sollte ein Wert seines RGB-Modells über 1 liegen, so wird die Farbe rot zurückgegeben.



## WorldInformation

In der Lektüre [1] wurde die Welt in Form einer `C++` Struktur dargestellt. Sie besitzt verschiedene Variablen um eine Szene zu modellieren (Lichter, Objekte, etc.). Unsere Applikation modelliert die Szene mithilfe von Unity. Dinge wie Lichter und Objekte sind in der Szene platziert und visuell bereits dargestellt. Um die Architektur trotzdem ein ähnliches Gerüst zu bieten, wurde die Klasse `WorldInformation` konzeptioniert und im Klassenattribute `GlobalWorld` instanziiert. Die Klasse besitzt alle Komponenten, die für den Raytracer vorhanden sein müssen. Zu Beginn der Applikation wird die Unity Szene in einem Parse-Vorgang in eine Instanz dieser Klasse abgebildet.

```c#
public AbstractTracer Tracer {get; set;}
public int MaxDepth { get; set; } = 10;
public Color BackgroundColor { get; set; } = Color.black;
```

 An erster Stelle wird der `Tracer` festgelegt, der für das Raytracing verwendet werden soll. Es wird `AbstractTracer` zur Definition benutzt, um jede Unterklasse per Polymorphie hier als Tracer einzusetzen. Aktuell wird ein `WhittedTracer` verwendet, um die Reflektionen der Metall- und Glassphären hervorzuheben. Für diese Reflektionen wurde in `MaxDepth` eine maximale Anzahl an Reflektionen festgelegt (10). Treffen Strahlen außerhalb der Szene auf bzw. schießen ins Leere, wird eine Hintergrundfarbe benötigt. Hier wurde mit `BackgroundColor` die Farbe Schwarz eingesetzt. 

```c#
public AbstractLight GlobalAmbientLight { get; set; }
public List<AbstractLight> GlobalLights { get; set; }
```

Die konstante Umgebungsbeleuchtung der Szene wird mithilfe einer `AmbientOccluder` Instanz realisiert und im Attribut `GlobalAmbientLight` gespeichert. Neben der konstanten Umgebungsbeleuchtung müssen die verschiedenen Lichtquellen der Szene in die `GlobalLights` Collection gespeichert werden. Unity bietet von Haus aus Game-Objekte an, um die Szene zu beleuchten. Diese Klassen bieten Informationen über Lichtstrahlrichtungen und Licht-Typ. Dies machen wir uns zu nutze, in dem wir die Szene-Lichter je nach Typ in die entsprechenden Komponenten der `AbstractLight` Teilarchitektur abbilden. 

```c#
foreach (Light l in Resources.FindObjectsOfTypeAll(typeof(Light)) as Light[])
{
    switch (l.type)
    {
		case LightType.Directional:
            ...
            DirectionalLight tmpLight = new DirectionalLight(...);   
            ...
                
    	case LightType.Point:
            ...
            PointLight tmpLight = new PointLight(...);    
            ...
    }
}
```



