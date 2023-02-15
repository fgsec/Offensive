# From System to Interactive Session

## Introdução

Em situações que temos execução via windows service com privilégios system, podemos ter como objetivo executar código na sessão interativa de algum usuário conectado na máquina. Nesse cenário, precisamos impersonar o token do nosso alvo e criar um processo em sua sessão. 

Este repositório contém o código 'RemoteExecution.cs' que implementa as funções necessárias para esse objetivo. A função 'OpenProcessAsUser' recebe a commandline, usuário alvo e variável de debug que são opcionais. 

Para simplificar a execução, o script em powershell 'payload.ps1' executa de forma dinâmica o .NET assembly.

## Funcionamento

Ao executar a função 'OpenProcessAsUser', executamos a 'GetTokenForTargetUser' para enumerar os processos explorer.exe e extrair o token associado com o processo, caso ele seja do nosso usuário ou qualquer um caso não seja indicado um usuário.

A extração do token ocorre a partir de algumas funções da API do Windows, como 'OpenProcess' e 'OpenProcessToken', posteriormente o token extraído é utilizado na função 'CreateProcessAsUser' com a configuração (Winsta0\\default) do desktop do usuário.