﻿using Microsoft.Research.SEAL.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Research.SEAL
{
    /// <summary>
    /// Class to store a plaintext element. The data for the plaintext is
    /// a polynomial with coefficients modulo the plaintext modulus. The degree
    /// of the plaintext polynomial must be one less than the degree of the
    /// polynomial modulus. The backing array always allocates one 64-bit word
    /// per each coefficient of the polynomial.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Memory Management
    /// The coefficient count of a plaintext refers to the number of word-size
    /// coefficients in the plaintext, whereas its capacity refers to the number 
    /// of word-size coefficients that fit in the current memory allocation. In 
    /// high-performance applications unnecessary re-allocations should be avoided 
    /// by reserving enough memory for the plaintext to begin with either by 
    /// providing the desired capacity to the constructor as an extra argument, or 
    /// by calling the reserve function at any time. 
    /// 
    /// When the scheme is scheme_type::BFV each coefficient of a plaintext is 
    /// a 64-bit word, but when the scheme is scheme_type::CKKS the plaintext is
    /// by default stored in an NTT transformed form with respect to each of the
    /// primes in the coefficient modulus. Thus, the size of the allocation that
    /// is needed is the size of the coefficient modulus (number of primes) times
    /// the degree of the polynomial modulus. In addition, a valid CKKS plaintext
    /// will also store the parms_id for the corresponding encryption parameters.
    /// </para>
    /// <para>
    /// Thread Safety
    /// In general, reading from plaintext is thread-safe as long as no other
    /// thread is concurrently mutating it. This is due to the underlying data
    /// structure storing the plaintext not being thread-safe.
    /// </para>
    /// </remarks>
    /// <seealso cref="Ciphertext">see Ciphertext for the class that stores ciphertexts.</seealso>
    public class Plaintext : NativeObject, IEquatable<Plaintext>
    {
        /// <summary>
        /// Constructs an empty plaintext allocating no memory.
        /// </summary>
        /// <param name="pool">The MemoryPoolHandle pointing to a valid memory pool</param>
        /// <exception cref="ArgumentException">if pool is uninitialized</exception>
        public Plaintext(MemoryPoolHandle pool = null)
        {
            IntPtr poolPtr = pool?.NativePtr ?? IntPtr.Zero;

            NativeMethods.Plaintext_Create(poolPtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Constructs a plaintext representing a constant polynomial 0. The
        /// coefficient count of the polynomial is set to the given value. The
        /// capacity is set to the same value.
        /// </summary>
        /// <param name="coeffCount">The number of (zeroed) coefficients in the
        /// plaintext polynomial</param>
        /// <param name="pool">The MemoryPoolHandle pointing to a valid memory pool</param>
        /// <exception cref="ArgumentException">if coeff_count is negative</exception>
        /// <exception cref="ArgumentException">if pool is uninitialized</exception>
        public Plaintext(int coeffCount, MemoryPoolHandle pool = null)
        {
            IntPtr poolPtr = pool?.NativePtr ?? IntPtr.Zero;

            NativeMethods.Plaintext_Create(coeffCount, poolPtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Constructs a plaintext representing a constant polynomial 0. The
        /// coefficient count of the polynomial and the capacity are set to the
        /// given values.
        /// </summary>
        /// <param name="capacity">The capacity</param>
        /// <param name="coeffCount">The number of (zeroed) coefficients in the
        /// plaintext polynomial</param>
        /// <param name="pool">The MemoryPoolHandle pointing to a valid memory pool</param>
        /// <exception cref="ArgumentException">if capacity is less than coeff_count</exception>
        /// <exception cref="ArgumentException">if coeff_count is negative</exception>
        /// <exception cref="ArgumentException">if pool is uninitialized</exception>
        public Plaintext(int capacity, int coeffCount,
                    MemoryPoolHandle pool = null)
        {
            IntPtr poolPtr = pool?.NativePtr ?? IntPtr.Zero;

            NativeMethods.Plaintext_Create(capacity, coeffCount, poolPtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Constructs a plaintext from a given hexadecimal string describing the
        /// plaintext polynomial.
        /// </summary>
        /// <remarks>
        /// The string description of the polynomial must adhere to the format
        /// returned by to_string(),
        /// which is of the form "7FFx^3 + 1x^1 + 3" and summarized by the following
        /// rules:
        /// 1. Terms are listed in order of strictly decreasing exponent
        /// 2. Coefficient values are non-negative and in hexadecimal format (upper
        /// and lower case letters are both supported)
        /// 3. Exponents are positive and in decimal format
        /// 4. Zero coefficient terms (including the constant term) may be (but do
        /// not have to be) omitted
        /// 5. Term with the exponent value of one must be exactly written as x^1
        /// 6. Term with the exponent value of zero (the constant term) must be written
        /// as just a hexadecimal number without exponent
        /// 7. Terms must be separated by exactly <space>+<space> and minus is not
        /// allowed
        /// 8. Other than the +, no other terms should have whitespace
        /// </remarks>
        /// <param name="hexPoly">The formatted polynomial string specifying the plaintext
        /// polynomial</param>
        /// <param name="pool">The MemoryPoolHandle pointing to a valid memory pool</param>
        /// <exception cref="ArgumentNullException">if hexPoly is null</exception>
        /// <exception cref="ArgumentException">if hex_poly does not adhere to the expected
        /// format</exception>
        /// <exception cref="ArgumentException">if pool is uninitialized</exception>
        public Plaintext(string hexPoly, MemoryPoolHandle pool = null)
        {
            if (null == hexPoly)
                throw new ArgumentNullException(nameof(hexPoly));

            IntPtr poolPtr = pool?.NativePtr ?? IntPtr.Zero;

            NativeMethods.Plaintext_Create(hexPoly, poolPtr, out IntPtr ptr);
            NativePtr = ptr;
        }

        /// <summary>
        /// Constructs a plaintext by initializing it with a pointer to a native object.
        /// </summary>
        /// <param name="plaintextPtr">Pointer to native Plaintext object</param>
        /// <param name="owned">Whether this instance owns the native pointer</param>
        internal Plaintext(IntPtr plaintextPtr, bool owned = true)
            : base(plaintextPtr, owned)
        {
        }

        /// <summary>
        /// Allocates enough memory to accommodate the backing array of a plaintext 
        /// with given capacity.
        /// </summary>
        /// <param name="capacity">The capacity</param>
        /// <exception cref="ArgumentException">if capacity is negative</exception>
        /// <exception cref="InvalidOperationException">if the plaintext is NTT transformed</exception>
        public void Reserve(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException(nameof(capacity));

            NativeMethods.Plaintext_Reserve(NativePtr, capacity);
        }

        /// <summary>
        /// Allocates enough memory to accommodate the backing array of the current
        /// plaintext and copies it over to the new location. This function is meant
        /// to reduce the memory use of the plaintext to smallest possible and can be
        /// particularly important after modulus switching.
        /// </summary>
        public void ShrinkToFit()
        {
            NativeMethods.Plaintext_ShrinkToFit(NativePtr);
        }

        /// <summary>
        /// Resets the plaintext. This function releases any memory allocated by the 
        /// plaintext, returning it to the memory pool.
        /// </summary>
        public void Release()
        {
            NativeMethods.Plaintext_Release(NativePtr);
        }

        /// <summary>
        /// Resizes the plaintext to have a given coefficient count. The plaintext 
        /// is automatically reallocated if the new coefficient count does not fit in 
        /// the current capacity. 
        /// </summary>
        /// <param name="coeffCount">The number of coefficients in the plaintext
        /// polynomial</param>
        /// <exception cref="ArgumentException">if coeff_count is negative</exception>
        /// <exception cref="InvalidOperationException">if the plaintext is NTT transformed</exception>
        public void Resize(int coeffCount)
        {
            if (coeffCount < 0)
                throw new ArgumentException(nameof(coeffCount));

            try
            {
                NativeMethods.Plaintext_Resize(NativePtr, coeffCount);
            }
            catch(COMException ex)
            {
                if ((uint)ex.HResult == NativeMethods.Errors.HRInvalidOperation)
                    throw new InvalidOperationException("Plaintext is NTT transformed", ex);
                throw;
            }
        }

        /// <summary>
        /// Copies a given plaintext to the current one.
        /// </summary>
        /// 
        /// <param name="assign">The plaintext to copy from</param>
        /// <exception cref="ArgumentNullException">if assign is null</exception>
        public void Set(Plaintext assign)
        {
            if (null == assign)
                throw new ArgumentNullException(nameof(assign));

            NativeMethods.Plaintext_Set(NativePtr, assign.NativePtr);
        }

        /// <summary>
        /// Sets the value of the current plaintext to the polynomial represented by the a given hexadecimal string.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// Sets the value of the current plaintext to the polynomial represented by the a given hexadecimal string.
        /// </para>
        /// <para>
        /// The string description of the polynomial must adhere to the format returned by <see cref="ToString()"/>,
        /// which is of the form "7FFx^3 + 1x^1 + 3" and summarized by the following rules:
        /// <list type="number">
        /// <item><description>Terms are listed in order of strictly decreasing exponent</description></item>
        /// <item><description>Coefficient values are non-negative and in hexadecimal format (upper and lower case letters are both supported)</description></item>
        /// <item><description>Exponents are positive and in decimal format</description></item>
        /// <item><description>Zero coefficient terms (including the constant term) may be (but do not have to be) omitted</description></item>
        /// <item><description>Term with the exponent value of one is written as x^1</description></item>
        /// <item><description>Term with the exponent value of zero (the constant term) is written as just a hexadecimal number without x or exponent</description></item>
        /// <item><description>Terms are separated exactly by &lt;space&gt;+&lt;space&gt;</description></item>
        /// <item><description>Other than the +, no other terms have whitespace</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="hexPoly">The formatted polynomial string specifying the plaintext polynomial</param>
        /// <exception cref="ArgumentException">if hexPoly does not adhere to the expected format</exception>
        /// <exception cref="ArgumentException">if the coefficients of hexPoly are too wide</exception>
        /// <exception cref="ArgumentNullException">if hexPoly is null</exception>
        public void Set(string hexPoly)
        {
            if (null == hexPoly)
                throw new ArgumentNullException(nameof(hexPoly));

            NativeMethods.Plaintext_Set(NativePtr, hexPoly);
        }

        /// <summary>
        /// Sets the value of the current plaintext to a given constant polynomial.
        /// </summary>
        /// 
        /// <remarks>
        /// Sets the value of the current plaintext to a given constant polynomial. The coefficient count
        /// is set to one.
        /// </remarks>
        /// <param name="constCoeff">The constant coefficient</param>
        public void Set(ulong constCoeff)
        {
            NativeMethods.Plaintext_Set(NativePtr, constCoeff);
        }


        /// <summary>
        /// Sets a given range of coefficients of a plaintext polynomial to zero.
        /// </summary>
        /// 
        /// <param name="startCoeff">The index of the first coefficient to set to zero</param>
        /// <param name="length">The number of coefficients to set to zero</param>
        /// <exception cref="ArgumentOutOfRangeException">if start_coeff is not within [0, CoeffCount)</exception>
        /// <exception cref="ArgumentOutOfRangeException">if length is negative or start_coeff + length is not within [0, CoeffCount)</exception>
        /// */
        public void SetZero(int startCoeff, int length)
        {
            if (startCoeff < 0)
                throw new ArgumentOutOfRangeException($"{nameof(startCoeff)} is negative");
            if (length < 0)
                throw new ArgumentOutOfRangeException($"{nameof(length)} is negative");

            try
            {
                NativeMethods.Plaintext_SetZero(NativePtr, startCoeff, length);
            }
            catch(COMException e)
            {
                if ((uint)e.HResult == NativeMethods.Errors.HRInvalidIndex)
                    throw new ArgumentOutOfRangeException("startCoeff or length out of range", e);
                throw;
            }
        }

        /// <summary>
        /// Sets the plaintext polynomial coefficients to zero starting at a given index.
        /// </summary>
        /// 
        /// <param name="startCoeff">The index of the first coefficient to set to zero</param>
        /// <exception cref="ArgumentOutOfRangeException">if start_coeff is not within [0, CoeffCount)</exception>
        public void SetZero(int startCoeff)
        {
            if (startCoeff < 0)
                throw new ArgumentOutOfRangeException($"{nameof(startCoeff)} is negative");

            try
            {
                NativeMethods.Plaintext_SetZero(NativePtr, startCoeff);
            }
            catch (COMException e)
            {
                if ((uint)e.HResult == NativeMethods.Errors.HRInvalidIndex)
                    throw new ArgumentOutOfRangeException("startCoeff or length out of range", e);
                throw;
            }
        }

        /// <summary>
        /// Sets the plaintext polynomial to zero.
        /// </summary>
        public void SetZero()
        {
            NativeMethods.Plaintext_SetZero(NativePtr);
        }

        /// <summary>
        /// Gets/set the value of a given coefficient of the plaintext polynomial.
        /// </summary>
        /// 
        /// <param name="coeffIndex">The index of the coefficient in the plaintext polynomial</param>
        /// <exception cref="ArgumentOutOfRangeException">if coeffIndex is not within [0, CoeffCount)</exception>
        public ulong this[int coeffIndex]
        {
            get
            {
                ulong result;
                NativeMethods.Plaintext_CoeffAt(NativePtr, coeffIndex, out result);
                return result;
            }
            set
            {
                NativeMethods.Plaintext_SetCoeffAt(NativePtr, coeffIndex, value);
            }
        }

        /// <summary>
        /// Returns whether the plaintext polynomial has all zero coefficients.
        /// </summary>
        public bool IsZero
        {
            get
            {
                NativeMethods.Plaintext_IsZero(NativePtr, out bool result);
                return result;
            }
        }

        /// <summary>
        /// Returns the capacity of the current allocation.
        /// </summary>
        public int Capacity
        {
            get
            {
                NativeMethods.Plaintext_Capacity(NativePtr, out int capacity);
                return capacity;
            }
        }

        /// <summary>
        /// Returns the coefficient count of the current plaintext polynomial.
        /// </summary>
        public int CoeffCount
        {
            get
            {
                NativeMethods.Plaintext_CoeffCount(NativePtr, out int result);
                return result;
            }
        }

        /// <summary>
        /// Returns the significant coefficient count of the current plaintext polynomial.
        /// </summary>
        public int SignificantCoeffCount
        {
            get
            {
                NativeMethods.Plaintext_SignificantCoeffCount(NativePtr, out int result);
                return result;
            }
        }

        /// <summary>
        /// Returns a human-readable string description of the plaintext polynomial.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returns a human-readable string description of the plaintext polynomial.
        /// </para>
        /// <para>
        /// The returned string is of the form "7FFx^3 + 1x^1 + 3" with a format summarized by the following:
        /// <list type="number">
        /// <item><description>Terms are listed in order of strictly decreasing exponent</description></item>
        /// <item><description>Coefficient values are non-negative and in hexadecimal format (hexadecimal letters are in upper-case)</description></item>
        /// <item><description>Exponents are positive and in decimal format</description></item>
        /// <item><description>Zero coefficient terms (including the constant term) are omitted unless the polynomial is exactly 0 (see rule 9)</description></item>
        /// <item><description>Term with the exponent value of one is written as x^1</description></item>
        /// <item><description>Term with the exponent value of zero (the constant term) is written as just a hexadecimal number without x or exponent</description></item>
        /// <item><description>Terms are separated exactly by &lt;space&gt;+&lt;space&gt;</description></item>
        /// <item><description>Other than the +, no other terms have whitespace</description></item>
        /// <item><description>If the polynomial is exactly 0, the string "0" is returned</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">if the plaintext is in NTT transformed form</exception>
        public override string ToString()
        {
            int length = 0;
            NativeMethods.Plaintext_ToString(NativePtr, ref length, outstr: null);

            StringBuilder buffer = new StringBuilder(length);
            NativeMethods.Plaintext_ToString(NativePtr, ref length, buffer);

            return buffer.ToString();
        }

        /// <summary>
        /// Returns a hash-code based on the value of the plaintext polynomial.
        /// </summary>
        public override int GetHashCode()
        {
            int coeffCount = CoeffCount;
            ulong[] coeffs = new ulong[coeffCount];
            for (int i = 0; i < coeffCount; i++)
            {
                coeffs[i] = this[i];
            }

            return Utilities.ComputeArrayHashCode(coeffs);
        }

        /// <summary>
        /// Check whether the current Plaintext is valid for a given SEALContext. If 
        /// the given SEALContext is not set, the encryption parameters are invalid, 
        /// or the Plaintext data does not match the SEALContext, this function returns 
        /// false. Otherwise, returns true.
        /// </summary>
        /// <param name="context">The SEALContext</param>
        /// <exception cref="ArgumentNullException">if context is null</exception>
        public bool IsValidFor(SEALContext context)
        {
            if (null == context)
                throw new ArgumentNullException(nameof(context));

            NativeMethods.Plaintext_IsValidFor(NativePtr, context.NativePtr, out bool result);
            return result;
        }

        /// <summary>
        /// Saves the plaintext to an output stream.
        /// </summary>
        /// 
        /// <remarks>
        /// Saves the plaintext to an output stream. The output is in binary format and not human-readable. 
        /// The output stream must have the "binary" flag set.
        /// </remarks>
        /// <param name="stream">The stream to save the plaintext to</param>
        /// <exception cref="ArgumentNullException">if stream is null</exception>
        /// <seealso cref="Load(Stream)">See Load() to load a saved plaintext.</seealso>
        /// */
        public void Save(Stream stream)
        {
            if (null == stream)
                throw new ArgumentNullException(nameof(stream));

            // First the ParmsId
            ParmsId.Save(stream);

            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Scale);
                writer.Write(CoeffCount);
                for (int i = 0; i < CoeffCount; i++)
                {
                    ulong data = this[i];
                    writer.Write(data);
                }
            }
        }

        /// <summary>
        /// Loads a plaintext from an input stream overwriting the current plaintext.
        /// No checking of the validity of the plaintext data against encryption
        /// parameters is performed. This function should not be used unless the 
        /// plaintext comes from a fully trusted source.
        /// </summary>
        /// <param name="stream">The stream to load the plaintext from</param>
        /// <exception cref="ArgumentNullException">if stream is null</exception>
        /// <exception cref="ArgumentException">if a valid plaintext could not be read from stream</exception>
        public void UnsafeLoad(Stream stream)
        {
            if (null == stream)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                ParmsId parms = new ParmsId();
                parms.Load(stream);
                ParmsId = parms;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    double scale = reader.ReadDouble();
                    int coeffCount = reader.ReadInt32();

                    Scale = scale;

                    ulong[] newData = new ulong[coeffCount];

                    for (int i = 0; i < coeffCount; i++)
                    {
                        newData[i] = reader.ReadUInt64();
                    }

                    NativeMethods.Plaintext_SwapData(NativePtr, coeffCount, newData);
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new ArgumentException("Stream ended unexpectedly", ex);
            }
            catch (IOException ex)
            {
                throw new ArgumentException("Could not read Plaintext", ex);
            }
        }

        /// <summary>
        /// Loads a plaintext from an input stream overwriting the current plaintext.
        /// The loaded plaintext is verified to be valid for the given SEALContext.
        /// </summary>
        /// <param name="context">The SEALContext</param>
        /// <param name="stream">The stream to load the plaintext from</param>
        /// <exception cref="ArgumentNullException">if either context or stream are null</exception>
        /// <exception cref="ArgumentException">if the context is not set or encryption
        /// parameters are not valid</exception>
        /// <exception cref="ArgumentException">if the loaded plaintext is invalid, or it is
        /// invalid for the context</exception>
        /// <seealso cref="Save(Stream)">See Save() to save a plaintext.</seealso>
        public void Load(SEALContext context, Stream stream)
        {
            if (null == context)
                throw new ArgumentNullException(nameof(context));
            if (null == stream)
                throw new ArgumentNullException(nameof(stream));

            UnsafeLoad(stream);

            if (!IsValidFor(context))
            {
                throw new ArgumentException("Plaintext data is invalid for context");
            }
        }

        /// <summary>
        /// Returns whether the plaintext is in NTT form.
        /// </summary>
        public bool IsNTTForm
        {
            get
            {
                NativeMethods.Plaintext_IsNTTForm(NativePtr, out bool result);
                return result;
            }
        }

        /// <summary>
        /// Returns a copy of parmsId. The parmsId must remain zero
        /// unless the plaintext polynomial is in NTT form.
        /// </summary>
        /// <seealso cref="EncryptionParameters">see EncryptionParameters for more information about parms_id.</seealso>
        public ParmsId ParmsId
        {
            get
            {
                ParmsId parms = new ParmsId();
                NativeMethods.Plaintext_GetParmsId(NativePtr, parms.Block);
                return parms;
            }

            private set
            {
                NativeMethods.Plaintext_SetParmsId(NativePtr, value.Block);
            }
        }

        /// <summary>
        /// Returns a reference to the scale. This is only needed when using the
        /// CKKS encryption scheme. The user should have little or no reason to ever
        /// change the scale by hand.
        /// </summary>
        public double Scale
        {
            get
            {
                NativeMethods.Plaintext_Scale(NativePtr, out double scale);
                return scale;
            }

            private set
            {
                NativeMethods.Plaintext_SetScale(NativePtr, value);
            }
        }

        /// <summary>
        /// Returns the currently used MemoryPoolHandle.
        /// </summary>
        public MemoryPoolHandle Pool
        {
            get
            {
                NativeMethods.Plaintext_Pool(NativePtr, out IntPtr pool);
                MemoryPoolHandle handle = new MemoryPoolHandle(pool);
                return handle;
            }
        }

        /// <summary>
        /// Returns whether or not the plaintext has the same semantic value as a given plaintext.
        /// </summary>
        /// <remarks>
        /// Returns whether or not the plaintext has the same semantic value as a given plaintext. Leading
        /// zero coefficients are ignored by the comparison.
        /// <param name="obj">The object to compare against</param>
        public override bool Equals(object obj)
        {
            Plaintext pt = obj as Plaintext;
            return Equals(pt);
        }

        /// <summary>
        /// Returns whether or not the plaintext has the same semantic value as a given plaintext.
        /// </summary>
        /// <remarks>
        /// Returns whether or not the plaintext has the same semantic value as a given plaintext. Leading
        /// zero coefficients are ignored by the comparison.
        /// </remarks>
        /// <param name="other">The plaintext to compare against</param>
        public bool Equals(Plaintext other)
        {
            if (null == other)
                return false;

            NativeMethods.Plaintext_Equals(NativePtr, other.NativePtr, out bool equals);
            return equals;
        }

        /// <summary>
        /// Destroy native object.
        /// </summary>
        protected override void DestroyNativeObject()
        {
            NativeMethods.Plaintext_Destroy(NativePtr);
        }
    }
}