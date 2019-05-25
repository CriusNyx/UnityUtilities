using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TimeTracker
{
    private float startTime;
    private DateTime dStartTime;

    public TimeTracker(float time)
    {
        startTime = time;
        dStartTime = DateTime.Now;
    }

    public float Time
    {
        get
        {
            return (float)(DateTime.Now - dStartTime).TotalSeconds + startTime;
        }
    }
}