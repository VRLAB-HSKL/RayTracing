# BTDF

. Für transparente Materialien muss der Anteil an Licht berechnet werden, der die Oberfläche durchdringt und in das Objekt selbst eindringt. 





## AbstractBTDF



Zu den vorhandenen Methoden die auch von `AbstractBRDF` vorgegeben werden, wurden 3 weitere Funktionen eingeführt, 

```c#
public abstract Color SampleF(RaycastHit hit, Vector3 wo, out Vector3 wt);
public abstract Color Rho(RaycastHit hit, Vector3 wo);
public abstract bool Tir(RaycastHit hit);
```

Die Methode `SampleF(..., out Vector3 wt)` gibt als _Output-Parameter_ den Richtungsvektor der Refkration, als dem Anteil des Lichtstrahls der in das Objekt eindringt. 

`Tir(...)` überprüft, ob eine __t__otale __i__nterne __R__eflektion vorliegt und liefert den entsprechenden Wahrheitswert.  





## PerfectTransmitterBTDF



```c#
public float KT;
public float IOR;
```





Mit der Methode