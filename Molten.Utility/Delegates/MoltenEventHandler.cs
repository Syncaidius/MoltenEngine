namespace Molten.Utility;

public delegate void MoltenEventHandler<T>(T o);

public delegate void MoltenEventHandler<T, U>(T o1, U o2);
