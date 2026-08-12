using Godot;

// NodeUtils.cs
// Helper para resolver hijos por nombre. El binario de Godot 4.7.1 embebido en
// este proyecto tiene un bug donde Node.GetNode/FindChild por nombre devuelven
// null (el nodo existe, GetChild(i) lo confirma, y child.Name imprime bien).
// Además la comparacion directa Node.Name (StringName) == string falla en este
// build, asi que se usa ToString() para comparar de forma robusta. Funciona
// tanto en ese build como en uno sano.
public static class NodeUtils
{
	public static T GetChildByName<T>(Node parent, string name) where T : class
	{
		if (parent == null) return null;
		foreach (Node child in parent.GetChildren())
		{
			if (child.Name.ToString() == name && child is T t)
				return t;
		}
		return null;
	}

	// Busca recursivamente en todo el árbol hijo de `root` (evita GetNode/GetParent
	// que fallan en el build embebido 4.7.1 por el bug de resolución de nodos).
	public static T FindInTree<T>(Node root, string name) where T : class
	{
		if (root == null) return null;
		foreach (Node child in root.GetChildren())
		{
			if (child.Name.ToString() == name && child is T t)
				return t;
			T found = FindInTree<T>(child, name);
			if (found != null) return found;
		}
		return null;
	}
}
