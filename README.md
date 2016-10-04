unity-bluconsole

TODO
* Not calling UnityLogHandler after play (for compiler errors)
  Steps to Reproduce:
    1º - Play UnityEditor
    2º - Press S or D or F
    3º - Quit play
    4º - Go to TestScript and make some parsing error (add a letter a to some line)

  Expected results: Compiler error
  Obtained results: No compile errors (normal Console get compiler erros)
