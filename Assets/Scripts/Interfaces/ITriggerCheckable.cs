// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerCheckable 
{
    // NOTE - Follow naming convention
    bool isAggroed { get; set; }
    bool isInStrikingDistance { get; set; }

    // NOTE - No point for these since you already exposed the set on the
    // properties on top
    void SetAggroStatus(bool isAggroed);

    void SetStrikingDistanceBool(bool isInStrikingDistance);
}
