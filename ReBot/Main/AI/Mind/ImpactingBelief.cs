using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReBot.Main.AI.Mind;

public record struct ImpactingBelief(float Cost, float Multiplier, BeliefRef BeliefRef);
