# BTDF

Eine weitere mathematische Funktion ist die _bidirectional transmittance distribution function_, kurz _BTDF_. Für transparente Materialien muss der Anteil an Licht berechnet werden, der die Oberfläche durchdringt und in das Objekt selbst eindringt bzw. wie dieses Licht gestreut wird. 

## AbstractBTDF

Zu den vorhandenen Methoden die auch von `AbstractBRDF` vorgegeben werden, wurden 3 weitere Funktionen eingeführt, 

```c#
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wt);
public abstract Color Rho(RaycastHit hit, Vector3 wo);
public abstract bool Tir(RaycastHit hit);
```

Die Methode `SampleF(..., out Vector3 wt)` gibt als _Output-Parameter_ den Richtungsvektor der Refkration, als dem Anteil des Lichtstrahls der in das Objekt eindringt.`Tir(...)` überprüft, ob eine __t__otale __i__nterne __R__eflektion vorliegt und liefert den entsprechenden Wahrheitswert.  

## PerfectTransmitterBTDF

```c#
public float KT;
public float IOR;
```

Zur Berechnung der Streuung muss ein Faktor existieren, der festlegt welcher Anteil des Lichts in das Objekt eintreten soll. Dies wird in dieser Unterklasse über den Faktor `KT` gesteuert. Die Variable `IOR` ist der Refraktionsindex (index of refraction), mit dessen Wert unterschiedliche transparente Materialien simuliert werden können (Eis, Wasser, Diamant, ...) .

## FresnelTransmitterBTDF

```c#
public float KT;
public float IOR;
```

In der Unterklasse `FresnelTransmitterBTDF` sind die gleichen Variablen wie in `PerfectTransmitterBTDF` vorhanden. Allein der Berechnungsweg der einzelnen Funktion stellt hier den Unterschied dar.



