using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReBot.Main.AI.Decision;

public record struct ImpactingAction(float Cost, float Multiplier, ActionRef ActionRef);
