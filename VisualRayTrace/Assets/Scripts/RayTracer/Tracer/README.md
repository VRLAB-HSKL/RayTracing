# Tracer

Um eine Szene mit geometrischen Objekten abzutasten, werden _Tracer_ benötigt. Innerhalb unserer Applikation ist vor allem die Technik des Raytracers von Bedeutung, da das Ziel der Applikation ist, diese Funktionalität zu visualisieren. Jedoch sorgt diese Architektur dafür, dass weitere Verfahren einfach implementiert und in den vorhanden Prozess eingepflegt werden können. Für die aktuelle Version wurde sich für die Implementierungen `RayCastTracer` (Single Ray) und `WhittedTracer` (Recursive Ray) entschieden. Die Lektüre [1] sieht auch Implementierung eines `PathTracer` vor. 



## AbstractTracer

Als Wurzelelement der Tracer Architektur steht die abstrakte Klasse `AbstractTracer`. Sie besteht aus $3$ virtuellen Funktionen mit gleichem Methodennamen. Ihre Signatur unterscheiden sich durch die geforderten Parameter. Meist ist es von der Unterklasse abhängig, welche Funktion zum Versenden der Strahlen verwendet wird.

```c#
public abstract Color TraceRay(Ray ray);
public abstract Color TraceRay(Ray ray, int depth);
public abstract Color TraceRay(Ray ray, float tmin, int depth);
```

Die erste Funktion nimmt einen `Ray` Parameter entgegen. In die Richtung dieses Strahls wird mithilfe der Unity Physics Engine Funktion `Physics.Raycast(...)` ein Strahl geschossen. Wird ein Objekt getroffen, so wird anhand des vorhandenen Materials des Objekts eine Farbe kalkuliert und von der Funktion zurückgegeben.

Die zweite Funktion `TraceRay(Ray ray, int depth)` nimmt einen weiteren `int` Parameter an. Dieser Parameter wird benutzt, um die Tiefe eines rekursiven Aufruf der Methode zu kontrollieren und bei einer erreichten Tiefe abzubrechen. Sie wird zur Simulation von Reflektionen und Transparenz bei Objekten mit entsprechenden Materialien verwendet. 



## WhittedTracer

Ein `WhittedTracer` rekonstruiert die Szene anhand eines 1980 von Turner Whitted vorgestellten Technik. Dieser Tracer bezieht nun reflektierende Strahlen in seinen Vorgang mit ein. Trifft ein Strahl auf ein Objekt mit reflektierenden Eigenschaften, so wird am Treffpunkt des Strahls mit dem Objekt ein weiterer Richtungsvektor kalkuliert, in dessen Richtung ein weiterer, reflektierter Strahl geschossen wird. Diese Strahlen können erneut auf reflektierende Oberflächen treffen und weitere Reflektion auslösen. Wie viele Reflektionen maximal durch einen initialen Strahl ausgelöst werden können, ist durch einen Schwellwert nach oben beschränkt. In unserer Anwendung ist dies mit der Konstante `RayTraceUtility.GlobalWorld.MaxDepth` definiert.

Der `WhittedTracer` verwendet die Methode `TraceRay(Ray ray, int depth)` um den Strahl zu verfolgen. Trifft ein Strahl auf ein reflektierendes Material, so wird die Methode rekursive mit einer inkrementierten Tiefe aufgerufen, also `TraceRay(reflectionRay, depth + 1)`. Wird der maximalen Wert an Reflektionen überschritten, so wird die Farbe schwarz zurückgegeben. 

```c#
if(depth > RayTraceUtility.GlobalWorld.MaxDepth)
{
    return Color.black;
}
else
{
    ...
}
```

