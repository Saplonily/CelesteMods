using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Celeste.Mod.DumpEffect;

namespace Celeste.Mod.DumpEffect;

public unsafe class DumpEffectModule : EverestModule
{
    public override void Load()
    {
        IL.Microsoft.Xna.Framework.Graphics.Effect.ctor_GraphicsDevice_ByteArray += Effect_ctor_GraphicsDevice_ByteArray;
    }

    private void Effect_ctor_GraphicsDevice_ByteArray(ILContext il)
    {
        ILCursor cur = new(il);
        cur.GotoNext(MoveType.After, ins => ins.MatchCall("Microsoft.Xna.Framework.Graphics.FNA3D", "FNA3D_CreateEffect"));
        cur.EmitLdloc0();
        cur.EmitDelegate((nint aint) =>
        {
            MOJOSHADER_effect* eft = (MOJOSHADER_effect*)aint;
            int count = eft->object_count;
            Console.WriteLine("--begin");
            for (int i = 0; i < count; i++)
            {
                var shd = eft->objects[i].shader;
                if (shd is null)
                {
                    Console.WriteLine($"{(nint)shd}");
                }
                else if (shd->parseData is null)
                {
                    Console.WriteLine($"{(nint)shd} -> {(nint)shd->parseData}");
                }
                else
                {
                    byte* utf8str = (byte*)shd->parseData->output;
                    int len = shd->parseData->output_len;
                    Console.WriteLine($"{(nint)shd} ->");
                    Console.WriteLine(Encoding.UTF8.GetString(new Span<byte>(utf8str, len)));
                }
            }
            Console.WriteLine("--end");
        });
    }

    public override void Unload()
    {
        IL.Microsoft.Xna.Framework.Graphics.Effect.ctor_GraphicsDevice_ByteArray -= Effect_ctor_GraphicsDevice_ByteArray;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MOJOSHADER_effect
{
    public int error_count;
    public void* errors;
    public int param_count;
    public void* @params;
    public int technique_count;
    public void* techniques;
    public int object_count;
    public MOJOSHADER_effectShader* objects;
    public void* current_technique;
    public int current_pass;
    public int restore_shader_state;
    public void* state_changes;
    public MOJOSHADER_effectShader* current_vert_raw;
    public MOJOSHADER_effectShader* current_pixl_raw;
    public void* current_vert;
    public void* current_pixl;
    public void* prev_vertex_shader;
    public void* prev_pixel_shader;
    public byte ctx;
}

public unsafe struct MOJOSHADER_effectShader
{
    public int type;
    public uint technique;
    public uint pass;
    public uint is_preshader;
    public uint preshader_param_count;
    public uint* preshader_params;
    public uint param_count;
    public uint* @params;
    public uint sampler_count;
    public void* samplers;
    public MOJOSHADER_glShader* shader;
}

public unsafe struct MOJOSHADER_glShader
{
    public MOJOSHADER_parseData* parseData;
    public uint handle;
    public uint refcount;
};

public unsafe struct MOJOSHADER_parseData
{
    public int error_count;
    public void* errors;
    public char* profile;
    public char* output;
    public int output_len;
    public int instruction_count;
    public int shader_type;
    public int major_ver;
    public int minor_ver;
    public char* mainfn;
    public int uniform_count;
    public void* uniforms;
    public int constant_count;
    public void* constants;
    public int sampler_count;
    public void* samplers;
    public int attribute_count;
    public void* attributes;
    public int output_count;
    public void* outputs;
    public int swizzle_count;
    public void* swizzles;
    public int symbol_count;
    public void* symbols;
    public void* preshader;
    public void* malloc;
    public void* free;
    public void* malloc_data;
}