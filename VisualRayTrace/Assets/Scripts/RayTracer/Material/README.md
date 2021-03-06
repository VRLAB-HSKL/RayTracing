# Material

Jedes Objekt in unserer Szene hat eine Beschaffenheit und Farbe, die seine Oberfläche überzieht. Diese Eigenschaften werden in einem Material gebündelt und den einzelnen Objekten zugewiesen. Wird ein Objekt von unserem Raytracer getroffen, wird die Berechnung der finalen Farbe davon beeinflusst, welches Material getroffen wurde. 



Diffuses Shading

$L_0(p, w_0) = p_{hh}\cdot L_i(p, w_i) = k_a c_d\cdot (l_s c_l)$



$L_i(p, w_i)$: ambient radiance (14.1)





Die Modellierung der Materialien werden über die `BRDF` Klassen realisiert. Diese sind eine weitere Komponente der Architektur und sind in diesem Repository unter `Assets/Scripts/RayTracer/BRDF` zu finden.  



## AbstractMaterial

Die abstrakte Oberklasse `AbstractMaterial` gibt die Schnittstelle vor, die von 



## MatteMaterial

Eine matte Oberfläche ist die simpelste Beschaffenheit, die eine Oberfläche annehmen kann. Hier wird eine komplett flache Oberfläche mit einer konstanten Farbe angenommen. Entsprechend werden Objekte mit diesem Material von unserem Raytracer dargestellt.

```c#
private Lambertian _ambientBRDF;
private Lambertian _diffuseBRDF;
```

ToDo:



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

Neben den von `MatteMaterial` bekannten `BRDF` Objekten wird ein Objekt der Klasse `GlossySpecular` hinzugefügt. Diese wird verwendet um die Highlights durch die Reflektion mit Lichtquellen darzustellen. 





Der Parameter `ks` steuert die Helligkeit des Highlights auf der Oberfläche des Objekts.

. Mit steigendem Exponent $e$ wird das Highlight der Oberfläche kleiner und die allgemeine Glanzintensität des Objekts stärker, da die Berechnung mit der Lichtquelle genauer wird. 

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

Die Klasse `ReflectiveMaterial` leitet von `PhongMaterial` ab, um die direkte Beleuchtung der Oberfläche zu berechnen. Das einzige was die Unterklasse selbst berechnen muss, ist der Einfluss auf die finale Farbe, die durch Reflektionen auf weitere Objekte der Szene entstehen.   

Um den Einfluss der Reflektionen zu bestimmen, enthält die Unterklasse ein `PerfectSpecularBRDF`. Um die Parameter dieser `BRDF` Implementierung zu setzten, wurden erneut Zugangsmethoden geschrieben.





## TransparentMaterial







# Quellen

[PhongRef] [Illumination of Computer Generated Pictures](https://users.cs.northwestern.edu/~ago820/cs395/Papers/Phong_1975.pdf), Bui Tuong Phong (1975)

