using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
/// <summary>
/// enum class holding Sources (Canvas, Frontend, Planning) for the RabbitMQ XML Headers.
/// </summary>
    public enum XMLSource
    {
        CANVAS,
        FRONTEND,
        PLANNING
    }
}
