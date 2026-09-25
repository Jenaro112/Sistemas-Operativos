using System;

Console.Write("Ingrese numero de ejercicio (1-6, 9, 10): ");
string? input = Console.ReadLine();

switch (input?.Trim())
{
    case "1":  Ejercicio1.Run();  break;
    case "2":  Ejercicio2.Run();  break;
    case "3":  Ejercicio3.Run();  break;
    case "4":  Ejercicio4.Run();  break;
    case "5":  Ejercicio5.Run();  break;
    case "6":  Ejercicio6.Run();  break;
    case "9":  Ejercicio9.Run();  break;
    case "10": Ejercicio10.Run(); break;
    default:
        Console.WriteLine("Ejercicio no valido. Opciones: 1-6, 9, 10");
        break;
}
