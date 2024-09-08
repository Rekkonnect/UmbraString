# UmbraString

A C# implementation of the Umbra string, aka German-styled string.

Many thanks to [ThePrimeagen](https://github.com/ThePrimeagen) for showcasing the Rust article below:

## References

- https://cedardb.com/blog/german_strings/
- https://tunglevo.com/note/an-optimization-thats-impossible-in-rust/

## Pre-Release Notice

This is a beta version of the library, testing out the performance, usability and correctness
of the implementation.

## Downloads

NuGet Package: [Rekkon.UmbraString](https://www.nuget.org/packages/Rekkon.UmbraString/)

## What's in?

- `UmbraString`, the classic Umbra string implementation
- `UmbraStringV2`, a variant allowing up to 15 bytes of content in the same struct
- `BigEndianUmbraStringV2`, the same variant but for big-endian systems
  - Currently untested, hence marked as `Obsolete`

Supported operations:
- Constructing from a `ReadOnlySpan<byte>`
- Comparing for equality
- Getting the `ReadOnlySpan<byte>` of the content
- Getting the length of the content
- Concatenating with another Umbra string
- Slicing the Umbra string

## Why use it?

The main appeal of the Umbra string is for unknown-length read-only strings that are often
compared. The implementation is also flexible enough allowing the construction from
a string with any encoding, including the default `System.String` type that uses UTF-16.

## Why not use it?

If you care a lot about performance and memory usage, it's best to evaluate whether a
generic implementation like the Umbra string suits you. Each Umbra string instance costs
16 bytes of memory, and is overkill for very short strings (up to 7 bytes of length).
It is thus not recommended to use the Umbra string for fixed-length strings or short
upper-bounded lengths. For example, to store 2/3-char country codes like "ITA", "USA",
it's better to implement a custom 4-byte struct serving that exact purpose.

Additionally, this is better used for strings whose encoding has a minimum char length of 1,
as the Umbra string stores the content in a byte array, in an encoding-agnostic manner.
Encodings like UTF-8, Windows-1252, and ASCII are good candidates. UTF-16 is strongly advised
against, as most of the time half the bytes are wasted.

Before taking any such decisions, make sure to carefully benchmark the performance of using
this library, *if* you have strong reason to optimize string access.

## Contribution

The library uses a lot of unsafe code, and needs careful inspection.
Suggestions for code improvements are always welcome.

## Performance

The purpose of the Umbra string is to be very fast for common equality checks, avoiding
the dereference to the content pointer as much as possible. Interacting with Umbra strings
like slicing, concatenating and constructing should also be very fast.
