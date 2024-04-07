using MonkeyLoader;
using MonkeyLoader.Patching;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreReleaseFixer
{
    internal class BasicPrePatcher : EarlyMonkey<BasicPrePatcher>
    {
        // The options for these should be provided by your game's game pack.
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        protected override IEnumerable<PrePatchTarget> GetPrePatchTargets()
        {
            yield return new PrePatchTarget(new AssemblyName("FrooxEngine"), "FrooxEngine.UIX.UIBuilder");
        }

        protected override bool Patch(PatchJob patchJob)
        {
            var uiBuilder = patchJob.Types.First();

            var vert = uiBuilder.Methods.First(m => m.Name == "VerticalLayout" && m.Parameters.Count == 5);

            var clone = new MethodDefinition("VerticalLayout", vert.Attributes, vert.ReturnType);

            clone.Parameters.Add(new ParameterDefinition(vert.Parameters[0].ParameterType));
            clone.Parameters.Add(new ParameterDefinition(vert.Parameters[1].ParameterType));
            clone.Parameters.Add(new ParameterDefinition(vert.Parameters[2].ParameterType));

            var proc = clone.Body.GetILProcessor();
            
            proc.Clear();

            clone.Body.Variables.Add(new VariableDefinition(vert.Parameters[3].ParameterType));

            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldarg_1);
            proc.Emit(OpCodes.Ldarg_2);
            proc.Emit(OpCodes.Ldarg_3);

            proc.Emit(OpCodes.Ldloca_S, (byte)0);
            proc.Emit(OpCodes.Initobj, vert.Parameters[3].ParameterType);
            proc.Emit(OpCodes.Ldloc_0);

            proc.Emit(OpCodes.Ldloca_S, (byte)0);
            proc.Emit(OpCodes.Initobj, vert.Parameters[4].ParameterType);
            proc.Emit(OpCodes.Ldloc_0);

            proc.Emit(OpCodes.Call, vert);
            proc.Emit(OpCodes.Ret);

            uiBuilder.Methods.Add(clone);

            patchJob.Changes = true;
            return true;
        }
    }
}