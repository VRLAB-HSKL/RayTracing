# Material

Jedes Objekt in unserer Szene hat eine Beschaffenheit und Farbe, die seine Oberfläche überzieht. Diese Eigenschaften werden in einem Material gebündelt und den einzelnen Objekten zugewiesen. Wird ein Objekt von unserem Raytracer getroffen, wird die Berechnung der finalen Farbe davon beeinflusst, welches Material getroffen wurde. Die Modellierung der Materialien werden über die `BRDF` und `BTDF` Klassen realisiert. Diese sind eine weitere Komponente der Architektur und sind in diesem Repository unter `Assets/Scripts/RayTracer/BRDF` bzw. `.../BTDF` zu finden.  



## AbstractMaterial

Die abstrakte Oberklasse `AbstractMaterial` gibt die Schnittstelle vor, die von den Materialien verwendet wird.  

```c#
public abstract Color Shade(RaycastHit hit, int depth);
public abstract Color AreaLightShade(RaycastHit hit);
public abstract Color PathShade(RaycastHit hit);
```

Je nach `Tracer` Typ wird eine der Funktionen verwendet, um Farben zu bestimmen. In der aktuellen Version der Applikation wird ein `WhittedTracer` eingesetzt, der vor allem die Funktion `Shade(...)` verwendet. Die Funktion  `AreaLightShade(...)` wird von einem `AreaLightingTracer` verwendet. `PathShade(...)` wird von einem `PathTracer` eingesetzt. Diese beiden Tracer-Varianten wurden in der aktuellen Version nicht implementiert, jedoch stehen die Funktionen innerhalb der Architektur zur Implementierung bereit. 



## MatteMaterial

Eine matte Oberfläche ist die simpelste Beschaffenheit, die eine Oberfläche annehmen kann. Hier wird eine komplett flache Oberfläche mit einer konstanten Farbe angenommen. Entsprechend werden Objekte mit diesem Material von unserem Raytracer dargestellt.

```c#
private Lambertian _ambientBRDF;
private Lambertian _diffuseBRDF;
```

Die beiden Attribute `_ambientBRDF` und `_diffuseBRDF` werden verwendet, um die Anteile der Umgebungsbeleuchtung und diffusen Beleuchtung an der finalen Farbe zu bestimmen. Je nach Lichtintensität nehmen die einzelnen Komponenten größeren Einfluss auf die Farbe.



```c#
public void SetKA(float ka);
public void SetKD(float kd);
public void SetCD(Color cd);
```

Um die Eigenschaften der `BRDF` Objekte unabhängig voneinander zu setzten, wurden _Setter_ Funktionen geschrieben. Um den Reflektionskoeffizient der umgebungsbasierten Beleuchtungsreflektion zu setzen wird `SetKA(float ka)` verwendet. Soll dieser für die diffuse Beleuchtungsreflektion zu setzen wird entsprechend `SetKD(float kd)` genutzt. Die Funktion `SetCD(Color cd)` setzt die Farbe der beiden `BRDF` Komponenten.

Die `Shade(...)` Funktion initialisiert die zu kalkulierende Farbe mithilfe der umgebungsbasierte `Lambertian` Komponenten. Danach wird über alle weiteren Lichtquellen in der Szene iteriert und ihre Beteiligung an der Farbe mit einberechnet. 



## PhongMaterial

Die `PhongMaterial` Klasse bildet eine neue Untergruppierung an weiteren Materialien, die einen glänzende Oberfläche besitzen. Im Gegensatz zu einem matten Material, haben diese Oberflächen ein Highlight durch eine Reflektion mit einer Lichtquelle. Diese Eigenschaften geben  solchen Materialien ein plastisches Aussehen. Der Name ist auf das _Phong Reflection Model_ von Bui Tuong Phon zurückzuführen, da hier alle $3$ Komponenten des Modells vorhanden sind (Ambient + Diffuse + Specular = Phong Reflection)  [PhongRef]

```c#
protected Lambertian _ambientBRDF;
protected Lambertian _diffuseBRDF;
protected GlossySpecular _specularBRDF;

protected Vector3 _rayDir;
```

Neben den von `MatteMaterial` bekannten `BRDF` Objekten wird ein Objekt der Klasse `GlossySpecular` hinzugefügt. Diese wird verwendet um die Highlights durch die Reflektion mit Lichtquellen darzustellen.  Der Parameter `ks` steuert die Helligkeit des Highlights auf der Oberfläche des Objekts. Mit steigendem Exponent $e$ wird das Highlight der Oberfläche kleiner und die allgemeine Glanzintensität des Objekts stärker, da die Berechnung mit der Lichtquelle genauer wird. 

```c#
public void SetKS(float ks)
{
    _specularBRDF.KS = ks;
}

public void SetExp(int exp)
{
    _specularBRDF.SpecularExponent = exp;
}
```

 



## ReflectiveMaterial

Die Klasse `ReflectiveMaterial` leitet von `PhongMaterial` ab, um die direkte Beleuchtung der Oberfläche zu berechnen. Das einzige was die Unterklasse selbst berechnen muss, ist der Einfluss auf die finale Farbe, die durch Reflektionen auf weitere Objekte der Szene entstehen. Sie wird zur Modellierung unserer Metalloberfläche verwendet.  

```c#
private PerfectSpecular _reflectiveBRDF;
```

Um den Einfluss der Reflektionen zu bestimmen, enthält die Unterklasse ein `PerfectSpecularBRDF`. Um die Parameter dieser `BRDF` Implementierung zu setzten, wurden mit `SetKR(...)` und `SetCR(...)` erneut Zugangsmethoden geschrieben.



## TransparentMaterial

 Bei transparenten Materialien wird ein Teil des einkommen Lichts reflektiert, jedoch tritt ein weiterer Teil des Lichts in das Objekt ein. Dieses Material wird für Strahlen verwendet, die die Glaskugeln in unserer Szene treffen. 

```c#
private PerfectTransmitterBTDF _specularBTDF;
```

Um den Lichtanteil, der in das Objekt eindringt, erneut in der finalen Farbberechnung mit einzubeziehen, wird ein `PerfectTransmitterBTDF` Attribut eingesetzt.  





# Quellen

[PhongRef] [Illumination of Computer Generated Pictures](https://users.cs.northwestern.edu/~ago820/cs395/Papers/Phong_1975.pdf), Bui Tuong Phong (1975)

