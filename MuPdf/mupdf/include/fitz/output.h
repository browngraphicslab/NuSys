#ifndef MUPDF_FITZ_OUTPUT_H
#define MUPDF_FITZ_OUTPUT_H

#include "fitz/system.h"
#include "fitz/context.h"
#include "fitz/buffer.h"

/*
	Generic output streams - generalise between outputting to a file,
	a buffer, etc.
*/
typedef struct fz_output_s fz_output;

/*
	fz_new_output_with_file: Open an output stream onto a FILE *.

	The stream does NOT take ownership of the FILE *.
*/
fz_output *fz_new_output_with_file(fz_context *, FILE *, int close);

/*
	fz_new_output_to_filename: Open an output stream to a filename.
*/
fz_output *fz_new_output_to_filename(fz_context *, const char *filename);

/*
	fz_new_output_with_buffer: Open an output stream onto a buffer.

	The stream does NOT take ownership of the buffer.
*/
fz_output *fz_new_output_with_buffer(fz_context *, fz_buffer *);

/*
	fz_printf: fprintf equivalent for output streams.
*/
int fz_printf(fz_context *, fz_output *, const char *, ...);

/*
	fz_puts: fputs equivalent for output streams.
*/
int fz_puts(fz_context *, fz_output *, const char *);

/*
	fz_write: fwrite equivalent for output streams.
*/
int fz_write(fz_context *, fz_output *out, const void *data, int len);

/*
	fz_putc: putc equivalent for output streams.
*/
void fz_putc(fz_context *, fz_output *out, char c);

/*
	fz_drop_output: Close a previously opened fz_output stream.

	Note: whether or not this closes the underlying output method is
	method dependent. FILE * streams created by fz_new_output_with_file
	are NOT closed.
*/
void fz_drop_output(fz_context *, fz_output *);

static inline int fz_write_int32be(fz_context *ctx, fz_output *out, int x)
{
	char data[4];

	data[0] = x>>24;
	data[1] = x>>16;
	data[2] = x>>8;
	data[3] = x;

	return fz_write(ctx, out, data, 4);
}

static inline int fz_write_int32le(fz_context *ctx, fz_output *out, int x)
{
	char data[4];

	data[0] = x;
	data[1] = x>>8;
	data[2] = x>>16;
	data[3] = x>>24;

	return fz_write(ctx, out, data, 4);
}

static inline void
fz_write_byte(fz_context *ctx, fz_output *out, int x)
{
	char data = x;

	fz_write(ctx, out, &data, 1);
}

/*
	fz_vfprintf: Our customised vfprintf routine. Same supported
	format specifiers as for fz_vsnprintf.
*/
int fz_vfprintf(fz_context *ctx, FILE *file, const char *fmt, va_list ap);
int fz_fprintf(fz_context *ctx, FILE *file, const char *fmt, ...);

/*
	fz_vsnprintf: Our customised vsnprintf routine. Takes %c, %d, %o, %s, %u, %x, as usual.
	Modifiers are not supported except for zero-padding ints (e.g. %02d, %03o, %04x, etc).
	%f and %g both output in "as short as possible hopefully lossless non-exponent" form,
	see fz_ftoa for specifics.
	%C outputs a utf8 encoded int.
	%M outputs a fz_matrix*. %R outputs a fz_rect*. %P outputs a fz_point*.
	%q and %( output escaped strings in C/PDF syntax.
	%ll{d,u,x} indicates that the values are 64bit.
	%z{d,u,x} indicates that the value is a size_t.
	%Z{d,u,x} indicates that the value is a fz_off_t.
*/
int fz_vsnprintf(char *buffer, int space, const char *fmt, va_list args);
int fz_snprintf(char *buffer, int space, const char *fmt, ...);

/*
	fz_tempfilename: Get a temporary filename based upon 'base'.

	'hint' is the path of a file (normally the existing document file)
	supplied to give the function an idea of what directory to use. This
	may or may not be used depending on the implementations whim.

	The returned path must be freed.
*/
char *fz_tempfilename(fz_context *ctx, const char *base, const char *hint);

#endif
