# BRDF

_Bidirectional reflectance distribution functions_, kurz _BRDF_, ist eine Familie von mathematischen Funktionen zur Modellierung der Reflektion von Licht von einer Oberfläche.





// spatially invariant



## AbstractBRDF

Die abstrakte Klasse `AbstractBRDF` gibt die Variablen und Methoden vor, die von allen `BRDF` Klassen verwendet werden. 





```c#
public abstract Color F(RaycastHit hit, Vector3 wo, Vector3 wi);
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi);
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf);
public abstract Color Rho(RaycastHit hit, Vector3 wo);
```

Die Methode `F(...)` berechnet den konkreten Funktionswert der Funktion und gibt sie in Form einer kalkulierten Farbe zurück. 



Die Methode `SampleF(...)` führt ein Sampling der Funktion durch und dient zusätzlich der Berechnung des Richtungsvektors von Reflektionen bei reflektierenden Materialien. Dieser wird in Form des _Output-Parameters_ `wi` ausgegeben. Das Sampling wird über den 

// ToDo: Sampling über _sampler Variable...



Mit `Rho(...)` wird die _bi-hemisphärische Reflektion_ $p_hh$ berechnet und zurückgegeben. Sie wird in der Farbberechnung der Umgebungsbeleuchtung eingesetzt, jedoch gibt sie in vielen Unterklassen auch einfach nur die Farbe schwarz zurück.  



## LambertianBRDF



Chapter 26



```c#
public float ReflectionCoefficient;
public float DiffuseColor;
```

Der diffuse Reflektion-Koeffizient kontrolliert, wieviel Licht von der Oberfläche reflektiert wird. Die Farbe bestimmt den Farbton.





Der Richtungsvektor der Reflektion in `SampleF(..., out Vector3 wi)` wird durch Sampling einer Hemisphäre über dem getroffenen Punkt bestimmt.





## PerfectSpecularBRDF

Die `PerfectSpecularBRDF` Komponente wird bei reflektierenden Materialien eingesetzt. 

Der Richtungsvektor für `SampleF(..., out Vector3 wi)` wird als komplett eindimensionale Reflektion (Spiegelung) am Punkt angenommen und entsprechend berechnet. 



Chapter 24



## GlossySpecularBRDF

Bei nicht perfekten Spiegelungen sind `GlossySpecularBRDF` Komponenten von Vorteil. Sie bilden Reflektionen ab, die einen zufälligen Anteil im resultierenden Richtungsvektor haben, um Materialien modellieren zu können, die keine glatte Oberfläche besitzen, bspw. eine Oberfläche mit rauer Beschaffenheit. 



Chapter 15

Chapter 25



Der `SampleF(..., out Vector3 wi, out float pdf)` Richtungsvektor wird erneut durch Sampling einer Heimsphäre bestimmt. Erneut im Punkt, jedoch so rotiert, dass die Sphäre in Richtung eines Spiegelungsrichtungsvektor orientiert ist. Hierbei wird die zweite Version der Funktion mit dem zusätzlichen _Output-Parameter_ `pdf`verwendet, da dies eine weitere Berechnung darstellt, die im Aufrufkontext der Funktion verwendet wird. 