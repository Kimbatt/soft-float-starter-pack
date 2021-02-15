# Soft float starter pack
Software implementation of floating point numbers and operations.  
Soft floats can be used for deterministic calculations, e.g. for physics simulation.  
They will give the same results every time, on every platform, on every processor.  
  
This repository uses the work of:
- [SoftFloat](https://github.com/CodesInChaos/SoftFloat), which implements basic soft float functionality
- [libm](https://github.com/rust-lang/libm), which implements various operations for floating point numbers, including square root, trigonometric functions, transcendental functions, etc. (ported to C#)

## How to use
The `sfloat` type is the main type that you'll need to use for soft float calculations.  
  
The `sfloat` type can be constructed in three ways:
- Explicit cast from float:
```csharp
sfloat a = (sfloat)1.0f;
sfloat b = (sfloat)(-123.456f);
sfloat c = (sfloat)float.PositiveInfinity;
sfloat d = (sfloat)float.NaN;
```
This cast is basically free, since the internal representations are identical.
- Explicit cast from int:
```csharp
sfloat a = (sfloat)1;
sfloat b = (sfloat)(-123);
sfloat c = (sfloat)int.MaxValue;
```
- Create from raw byte representation
```csharp
sfloat a = sfloat.FromRaw(0x00000000); // == 0
sfloat b = sfloat.FromRaw(0x3f800000); // == 1
sfloat c = sfloat.FromRaw(0xc2f6e979); // == -123.456
sfloat d = sfloat.FromRaw(0x7f800000); // == Infinity
```
This cast is also basically free, it's just the byte representation of the value.

The rest of the operations work just like with floats (addition, multiplication, etc.).  
Note that you should always use a float literal (or a variable that was assigned a float literal before) for explicit casts from floats, since any operation done on floats can be non-deterministic.
```csharp
// OK
float a = 1.0f;
sfloat b = (sfloat)a + (sfloat)123.456f;


// NOT OK
float a = 1.0f;
sfloat b = (sfloat)(a + 123.456f); // <-- float addition here, which may be non-deterministic
```

### Using libm

You can use libm just like a regular mathematics library:
```csharp
sfloat x = (sfloat)2.0f;
sfloat squareRoot = libm.sqrtf(x);

sfloat c = libm.cosf((sfloat)3.1415f);

sfloat e = libm.expf((sfloat)1.0f);
```
All functions have the f suffix, which comes from the rust libm implementation. You can rename them if you want to.
