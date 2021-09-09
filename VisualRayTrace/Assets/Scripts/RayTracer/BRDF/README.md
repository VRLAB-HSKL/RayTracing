# BRDF

_Bidirectional reflectance distribution functions_, kurz _BRDF_, ist eine mathematischen Funktion zur Modellierung des Reflexionsverhaltens von Licht auf einer Oberfläche. Diese bidirektionale Reflektanzverteilungsfunktion betrachtet den Eingangswinkel von einzelnen Lichtstrahlen und ermittelt die Streuung. Hiermit können verschieden Materialien modelliert werden und gesteuert werden, wie stark ein Material reflektiert. Hierzu wird in der Computergrafik folgende Grundformel verwendet:

$f_r (w_i, w_o) \equiv \frac{dL_0(w_0)}{L_i(w_i)\cos(\theta_i)d w_i}$

$w_i$: Eingangswinkel

$w_o$: Ausgangswinkel

$\theta_i$: Azimutwinkel des Eingangswinkels (Winkel zwischen Eingangswinkel und Normale) 

$dL_0$: Differentielle Strahlungsdichte (Radiance)

$L_i$: Bestrahlungsstärke (Irradiance)



## AbstractBRDF

Die abstrakte Klasse `AbstractBRDF` gibt die Variablen und Methoden vor, die von allen `BRDF` Klassen verwendet werden. 

```c#
public abstract Color F(RaycastHit hit, Vector3 wo, Vector3 wi);
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi);
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wi, out float pdf);
public abstract Color Rho(RaycastHit hit, Vector3 wo);
```

Die Methode `F(...)` berechnet den konkreten Funktionswert der Funktion und gibt sie in Form einer kalkulierten Farbe zurück. Die Methode `SampleF(...)` führt ein Sampling der Funktion durch und dient zusätzlich der Berechnung des Richtungsvektors von Reflektionen bei reflektierenden Materialien. Dieser wird in Form des _Output-Parameters_ `wi` ausgegeben. Das Sampling wird über eine `AbstractSampler` Variable durchgeführt. Mit `Rho(...)` wird die _bi-hemisphärische Reflektion_ $p_hh$ berechnet und zurückgegeben. Sie wird in der Farbberechnung der Umgebungsbeleuchtung eingesetzt, jedoch gibt sie in vielen Unterklassen auch einfach nur die Farbe schwarz zurück.  

## LambertianBRDF

```c#
public float ReflectionCoefficient;
public float DiffuseColor;
```

Der diffuse Reflektion-Koeffizient kontrolliert, wieviel Licht von der Oberfläche reflektiert wird. Die Farbe bestimmt den Farbton. Der Richtungsvektor der Reflektion in `SampleF(..., out Vector3 wi)` wird durch Sampling einer Hemisphäre über dem getroffenen Punkt bestimmt.

## PerfectSpecularBRDF

Die `PerfectSpecularBRDF` Komponente wird bei reflektierenden Materialien eingesetzt. Der Richtungsvektor für `SampleF(..., out Vector3 wi)` wird als komplett eindimensionale Reflektion (Spiegelung) am Punkt angenommen und entsprechend berechnet. 

## GlossySpecularBRDF

Bei nicht perfekten Spiegelungen sind `GlossySpecularBRDF` Komponenten von Vorteil. Sie bilden Reflektionen ab, die einen zufälligen Anteil im resultierenden Richtungsvektor haben, um Materialien modellieren zu können, die keine glatte Oberfläche besitzen, bspw. eine Oberfläche mit rauer Beschaffenheit. 

Der `SampleF(..., out Vector3 wi, out float pdf)` Richtungsvektor wird erneut durch Sampling einer Heimsphäre bestimmt. Erneut im Punkt, jedoch so rotiert, dass die Sphäre in Richtung eines Spiegelungsrichtungsvektor orientiert ist. Hierbei wird die zweite Version der Funktion mit dem zusätzlichen _Output-Parameter_ `pdf`verwendet, da dies eine weitere Berechnung darstellt, die im Aufrufkontext der Funktion verwendet wird. 