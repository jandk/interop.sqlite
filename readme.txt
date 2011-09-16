This is a repository for my own SQLite interop library.

I made this because I wasn't happy with any of the libraries that were
available.

I wanted a simple way to enumerate objects straight out of the database,
so I created SQLite3Enumerator. This class takes a result, and enumerates the
rows from the result as an object type you choose. This class has been written
to be as performant as possible, without any unnecessary boxing and unboxing.

The library does need a lot of work still, as there is a lot of functionality
missing. But this will be implemented in the future.

For now it only works on x86, with an x86 compiled version of sqlite available
on http://sqlite.org.

The sample program in the Test project, creates a new database in your
"My Documents" folder, creates a simple table, and fills it with some data. It
then selects this data, and populates some objects with it.
