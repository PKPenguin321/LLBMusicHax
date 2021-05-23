using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;

namespace ASMRewriter
{
    class asm
    {
        public static void SetClassPublic(AssemblyDefinition _adef, string _class)
        {
            TypeDefinition tdef = null;

            if (tdef == null) tdef = _adef.MainModule.GetType(_class);


            if (tdef != null)
            {
                if (!tdef.IsPublic)
                {
                    tdef.IsPublic = true;
                    Console.WriteLine("Set class " + _class + " to public");
                } else Console.WriteLine("Class " + _class + " already is public");
            } else
            {
                Console.WriteLine("Could not find class " + _class);
                return;
            }
        }

        public static void SetStructPublic(AssemblyDefinition _adef, string _class, string _struct)
        {
            TypeDefinition tdef = _adef.MainModule.GetType(_class);

            if (tdef != null)
            {
                foreach (TypeDefinition sdef in tdef.NestedTypes)
                {
                    if (sdef.Name == _struct)
                    {
                        if (!sdef.IsPublic)
                        {
                            sdef.IsPublic = true;
                            Console.WriteLine("Set method " + _struct + " in class " + _class + " to Public");
                        } else Console.WriteLine("Method " + _struct + " in class " + _class + " already is Public");
                    }
                }
            }

        }

        public static void SetMethodType(AssemblyDefinition _adef, string _class, string _method, ChangeType _changetype)
        {
            TypeDefinition tdef = null;

            if (tdef == null) tdef = _adef.MainModule.GetType(_class);

            if (tdef != null)
            {
                foreach (MethodDefinition method in tdef.Methods)
                {
                    if (method.Name == _method)
                    {
                        switch (_changetype)
                        {
                            case ChangeType.Public:
                                if (!method.IsPublic)
                                {
                                    method.IsPublic = true;
                                    Console.WriteLine("Set method " + _method + " in class " + _class + " to Public");
                                } else Console.WriteLine("Method " + _method + " in class " + _class + " already is Public");
                                break;
                            case ChangeType.Virtual:
                                if (!method.IsVirtual)
                                {
                                    method.IsVirtual = true;
                                    Console.WriteLine("Set method " + _method + " in class " + _class + " to Virtual");
                                } else Console.WriteLine("Method " + _method + " in class " + _class + " already is Virtual");
                                break;
                            case ChangeType.Abstract:
                                if (!method.IsAbstract)
                                {
                                    method.IsAbstract = true;
                                    Console.WriteLine("Set method " + _method + " in class " + _class + " to Abstract");
                                }
                                else Console.WriteLine("Method " + _method + " in class " + _class + " already is Abstract");
                                break;
                            default:
                                Console.WriteLine("Error on ChangeType call");
                                break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find class " + _class);
                return;
            }
        }

        public static void SetFieldPublic(AssemblyDefinition _adef, string _class, string _field)
        {
            TypeDefinition tdef = null;

            if (tdef == null) tdef = _adef.MainModule.GetType(_class);

            if (tdef != null)
            {
                FieldDefinition fdef = null;
                foreach (FieldDefinition field in tdef.Fields) if (field.Name == _field) fdef = field;

                if (fdef != null)
                {
                    if (!fdef.IsPublic)
                    {
                        fdef.IsPublic = true;
                        Console.WriteLine("Set field " + _field + " in class " + _class + " to public");
                    } else Console.WriteLine("Field " + _field + " in class " + _class + " already is public");
                } else
                {
                    Console.WriteLine("Could not find field " + _field + " in " + _class);
                    return;
                }
            } else
            {
                Console.WriteLine("Could not find class " + _class);
                return;
            }
        }

        public static void InjectCallToMethod(AssemblyDefinition _gameAdef, string _injectClass, string _injectMethod, int _entryNr, AssemblyDefinition _modAdef, string _modClass, string _modMethod, InjectFlags _flag)
        {
            TypeDefinition gameClass = _gameAdef.MainModule.GetType(_injectClass);
            MethodDefinition gameMethod = null;

            if (gameClass != null)
            {
                var i = 0;
                foreach (MethodDefinition method in gameClass.Methods)
                {
                    if (method.Name == _injectMethod)
                    {
                        if (i == _entryNr)
                        {
                            Console.WriteLine("Selected method to inject into: " + method.FullName);
                            gameMethod = method;
                            break;
                        }
                        else i++;
                    }
                }
            } else
            {
                Console.WriteLine("Could not find game class " + _injectClass);
                return;
            }


            if (gameMethod == null)
            {
                Console.WriteLine("Could not find game method " + _injectMethod);
                return;
            }

            MethodDefinition modMethod = null;

            foreach (ModuleDefinition module in _modAdef.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        //Console.WriteLine(method.Name);
                        if (method.Name == _modMethod)
                        {
                            modMethod = method;
                        }
                    }
                }
            }

            if (modMethod == null)
            {
                Console.WriteLine("Could not find mod method " + _modMethod);
                return;
            }

            try
            {
                InjectionDefinition injector = new InjectionDefinition(gameMethod, modMethod, _flag);
                injector.Inject(0, null, InjectDirection.Before);
                Console.WriteLine("Injection into " + gameMethod.ToString() + " OK ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
