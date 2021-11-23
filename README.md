# HTTPConsole
a BurpSuite knockoff I made because I don't want to download BurpSuite

Methods:

All HTTP methods (GET, HEAD, POST, PUT, DELETE, CONNECT, OPTIONS, TRACE, PATCH)

URL: changes the target URL.

VERBATIM: sets HTTP request method to prompt response. Used to set one of these commands as the HTTP method.

RUN: reads and runs commands from a given file.

RUN_DEBUG: like RUN, but displays prompts while running.

PIPE: sends program output to file.

LOOP: starts a loop. (explanation below)

CLEAR: clears the screen.

END: closes the program.

LOOP Instructions:

Display counter (y/n): determines whether or not the current i value of each iteration should be printed to output.


Valid for statements:

for $range $increment(optional)

examples:

for 100 (for (int i = 0; i <= 100; i++))

for 30..0 (for (int i = 30; i >= 0; i--))

for 64 *2 (for (int i = 1; i <= 64; i *= 2))

for 0.. (for (int i = 0; true; i++))

for 50 2 (for (int i = 0; i <= 50; i++))

for 128 <1 (for (int i = 1; i <= 128; i <<= 2))


Valid ranges:

any two numbers separated with ".."

any one number (translates to 0..n)

ranges are start and end inclusive.

examples: 

0.. (infinite starting at 0)

10..20 (starting at 10, ending at 20)

5..-5 (starting at 5, ending at -5)

100 (starting at 0, ending at 100)


Valid increments:

optionally any operator (+-*/^<>) optionally followed by an integer.

if no integer is provided, defaults to 2 if *, /, ^, <, or >, otherwise 1.

if no increment is provided, defaults to +1 or -1 as appropriate.

examples:

+1 (i++)

\*2 (i *= 2)

<1 (i << 1)

\>2 (i >> 2)

^3 (pow(i, 3))

\- (i--)

2 (i += 2 or i -= 2, as appropriate)


Special Instructions:

take the form:

display/break key/header/content/code/uri (!)contains/is value


display: only display output of this iteration evaluates to true.

break: break loop if output of this iteration evaluates to true.


key: all HTTP headers received

header: all HTTP header values received

content: HTTP content received

code: HTTP response code received

uri: URI received


contains: returns true if specified source contains value

is: returns true if specified source is equivalent to value

(can optionally place ! before this argument to invert result)


examples:

display content !contains very special though (displays HTTP response if it's content does not contain the text "very special though")

break content contains not a valid cookie (breaks loop if HTTP response content contains "not a valid cookie")

example for loop to solve (http://mercury.picoctf.net:6418/):
```
for 100
break content contains appear to be a valid cookie
display content !contains Not very special though

get
cookie
name=$i


end
```