using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using YMU.Core;

/// <summary>
/// Contains helper classes to parse and deserialize XML files.
/// </summary>
namespace YMU.Parsing {
    /// <summary>
    /// Creates lightweight DOM structure of provideded XML string.
    /// 
    /// Xml Parser can be used to create lightweight object model of the given XML file.
    /// It is very effective because it process whole XML file by single pass without recursion.
    /// </summary>
    public class ParserXml {
		#region Nested types

		/// <summary>
		/// Class which represents XML attribute.
		/// </summary>
		public class Attribute : ICloneable {
			#region Constructors

			/// <summary>
			/// Default constructor which initializes a new instance of the <see cref="Attribute"/> class.
			/// </summary>
			public Attribute() {
				Name = string.Empty;
				Value = string.Empty;
			}

			/// <summary>
			/// Default constructor which initializes a new instance of the <see cref="Attribute"/> class.
			/// </summary>
			/// <param name="reader">Attribute data provider.</param>
			public Attribute(XmlTextReader reader) : base() {
				Name = reader.Name;
				reader.Read();
				Value = reader.Name;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets or sets attribute's name.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets attribute's value.
			/// </summary>
			public string Value { get; set; }

			#endregion Properties

			#region ICloneable implementation

			/// <summary>
			/// Clone this instance.
			/// </summary>
			object ICloneable.Clone() {
				return this.Clone();
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			public object Clone() {
				return new Attribute() { Name = Name, Value = Value };
			}

			#endregion
		}

		/// <summary>
		/// Class which represents XML tag.
		/// </summary>
		public class Tag : ICloneable {
			#region Private members

			/// <summary>
			/// Set of tag's attributes.
			/// </summary>
			//private List<Attribute> mAttributes = new List<Attribute>();
			private Dictionary<string, Attribute> mAttributes = new Dictionary<string, Attribute>(10, StringComparer.InvariantCultureIgnoreCase);

			/// <summary>
			/// Set of tag's children.
			/// </summary>
			private List<Tag> mChildren = new List<Tag>();

			private static readonly Tag mEmpty = new ParserXml.Tag();

			#endregion Private members

			#region Constructors

			/// <summary>
			/// Default constructor which initializes a new instance of the <see cref="Tag"/> class.
			/// </summary>
			public Tag() { }

			/// <summary>
			/// Default constructor which initializes a new instance of the <see cref="Tag"/> class.
			/// </summary>
			/// <param name="reader">Tag data provider.</param>
			public Tag(XmlTextReader reader) : this() {
				Tag current = this;
				ReadNext(reader);
				if(reader.NodeType == XmlNodeType.Element) {
					current.Name = reader.Name;
					current.ParseAttributes(reader);
				}

				while(ReadNext(reader)) {
					if(reader.NodeType == XmlNodeType.Element) {
						if(reader.IsEmptyElement) // Empty tag - without value or children;
						{
							Tag tag = new Tag() { Name = reader.Name };
							tag.ParseAttributes(reader);
							current.Add(tag);
						} else // Tag with children;
							{
							Tag tag = new Tag() { Name = reader.Name };
							tag.ParseAttributes(reader);
							current.Add(tag);
							current = tag;
						}
					} else if(reader.NodeType == XmlNodeType.Text) {
						current.Value = reader.Value;
					} else if(reader.NodeType == XmlNodeType.EndElement) {
						current = current.Parent;
					}
				}
			}

			#endregion Constructors

			#region Properties

			/// <summary>
			/// Gets or sets tag's name.
			/// </summary>
			/// <value>Name of the tag.</value>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets the tag's value.
			/// </summary>
			/// <value>Value of the tag.</value>
			public string Value { get; set; }

			/// <summary>
			/// Gets tag's parent.
			/// </summary>
			/// <value>Parent of the tag.</value>
			public Tag Parent { get; private set; }

			/// <summary>
			/// Gets list of attributes provided by tag.
			/// </summary>
			/// <value>Attributes of the tag.</value>
			public IEnumerable<Attribute> Attributes { get { return mAttributes.Values; } }

			/// <summary>
			/// Gets tag children.
			/// </summary>
			/// <value>Children of the tag.</value>
			public IEnumerable<Tag> Children { get { return mChildren; } }

			/// <summary>
			/// Gets the empty instance of Tag class.
			/// </summary>
			/// <value>The empty instance of Tag class.</value>
			public static Tag Empty { get { return mEmpty; } }

			#endregion Properties

			#region Public methods

			/// <summary>
			/// Add specified tag as a child.
			/// </summary>
			/// <param name="tag">Tag which become child of the current one.</param>
			public void Add(Tag tag) {
				if(tag == null)
					return;

				if(string.IsNullOrEmpty(tag.Name))
					return;

				tag.Parent = this;
				mChildren.Add(tag);
			}

			/// <summary>
			/// Add specified attribute to the tag.
			/// </summary>
			/// <param name="attribute">Specifies new attribute of the current tag.</param>
			public void Add(Attribute attribute) {
				if(attribute == null)
					return;

				if(string.IsNullOrEmpty(attribute.Name))
					return;

				mAttributes.Add(attribute.Name, attribute);
			}

			/// <summary>
			/// Finds attribute by its name.
			/// </summary>
			/// <param name="name">Name of the attribute which should be found.</param>
			/// <returns>The founded attribute or null.</returns>
			public Attribute FindAttribute(string name) {
				foreach(var a in Attributes) {
					if(string.Equals(a.Name, name, System.StringComparison.InvariantCultureIgnoreCase))
						return a;
				}
				return null;
			}

			/// <summary>
			/// Finds tag by its name from the children list.
			/// </summary>
			/// <param name="name">Name of the tag which should be found.</param>
			/// <returns>The founded tag or null.</returns>
			public Tag FindTag(string name) {
				if(string.Equals(Name, name, System.StringComparison.InvariantCultureIgnoreCase))
					return this;
				foreach(var c in Children) {
					if(string.Equals(c.Name, name, System.StringComparison.InvariantCultureIgnoreCase))
						return c;
				}
				return null;
			}

			/// <summary>
			/// Finds set of tags by their name from the children list.
			/// </summary>
			/// <param name="name">Name of the tags which should be found.</param>
			/// <returns>The founded tags or empty set.</returns>
			public IEnumerable<Tag> FindTags(string name) {
				List<Tag> result = new List<Tag>();
				if(string.Equals(Name, name, System.StringComparison.InvariantCultureIgnoreCase)) {
					result.Add(this);
					return result;
				}

				foreach(var c in Children) {
					if(string.Equals(c.Name, name, System.StringComparison.InvariantCultureIgnoreCase))
						result.Add(c);
				}
				return result;
			}

			/// <summary>
			/// Remove specified attribute from tag.
			/// </summary>
			/// <param name="attribute">Attribute which should me removed from tag.</param>
			public bool Remove(Attribute attribute) {
				return mAttributes.Remove(attribute.Name);
			}

			/// <summary>
			/// Remove specified tag from children.
			/// </summary>
			/// <param name="attribute">Tag which should me removed from children.</param>
			public bool Remove(Tag tag) {
				var tags = mChildren.FindAll(c => tag.Name == c.Name);
				foreach(Tag t in tags) {
					t.Parent = null;
					mChildren.Remove(t);
				}
				return tags.Count > 0;
			}

			#endregion Public methods

			#region Private methods

			private bool ReadNext(XmlTextReader reader) {
				while(reader.Read()) {
					if(reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
						return true;
					else if(reader.NodeType == XmlNodeType.EndElement)
						return true;
					else if(reader.NodeType == XmlNodeType.Text)
						return true;
				}
				return false;
			}

			private void ParseAttributes(XmlTextReader reader) {
				if(reader.HasAttributes) {
					while(reader.MoveToNextAttribute())
						mAttributes.Add(reader.Name, new Attribute { Name = reader.Name, Value = reader.Value });
				}
			}

			#endregion Private methods

			#region ICloneable implementation

			/// <summary>
			/// Clone this instance.
			/// </summary>
			object ICloneable.Clone() {
				return this.Clone();
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			public Tag Clone() {
				var result = new Tag() { Name = Name, Value = Value };

				foreach(var attribute in Attributes) {
					result.Add(attribute.Clone() as Attribute);
				}

				foreach(var tag in Children) {
					result.Add(tag.Clone() as Tag);
				}

				return result;
			}

			#endregion
		}

		#endregion Nested types

		#region Constructor 

		/// <summary>
		/// Default constructor which initializes a new instance of the <see cref="ParserXml"/> class.
		/// </summary>
		public ParserXml() { }

		#endregion Constructor

		#region Public properties

		/// <summary>
		/// Gets the root tag of the XML document.
		/// </summary>
		/// <value>The root tag of the XML document.</value>
		public Tag Root { get; private set; }

		#endregion Public properties

		#region Public methods

		/// <summary>
		/// Loads XML object model from stream.
		/// </summary>
		/// <param name="stream">Stream which is used as a source for XML object model generation process.</param>
		public void Load(Stream stream) {
			XmlTextReader reader = new XmlTextReader(stream);
			Root = new Tag(reader);
		}

		/// <summary>
		/// Loads XML object model from string.
		/// </summary>
		/// <param name="stream">String which is used as a source for XML object model generation process.</param>
		public void Load(string text) {
			MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(TruncateTill(text, '<')));
			Load(stream);
		}

		/// <summary>
		/// Finds tag by its name from the children list.
		/// </summary>
		/// <param name="name">Name of the tag which should be found.</param>
		/// <returns>The founded tag or null.</returns>
		public Tag FindTag(string name) {
			return Root.FindTag(name);
		}

		/// <summary>
		/// Writes tag information into log.
		/// </summary>
		public void Log() {
			Log(Root);
		}

		/// <summary>
		/// Writes tag information into log beginning from specified tag.
		/// </summary>
		/// <param name="root">Tag beginning from which debug information should be generated.</param>
		/// <param name="level">Deepness of the tag in hierarchy.</param>
		public void Log(ParserXml.Tag root, int level = 0) {
			if(string.IsNullOrEmpty(root.Value))
				Console.WriteLine("[XML] Level: {0}, Name: {1}, Attributes: {2};", level, root.Name, root.Attributes.Count());
			else
				Console.WriteLine("[XML] Level: {0}, Name: {1}, Value: {2}, Attributes: {3};", level, root.Name, root.Value, root.Attributes.Count());
			foreach(var tag in root.Children) {
				Log(tag, level + 1);
			}
		}
		#endregion Public methods

		private string TruncateTill(string source, char till) {
			for(int i = 0; i < source.Length; i++) {
				if(source[i] == till) {
					if(i == 0)
						return source;
					else
						return source.Remove(0, i);
				}
			}
			return source;
		}
	}

	/// <summary>
	/// Deserialize data from XML.
	/// </summary>
	public class DeserializerXml {

		private Dictionary<string, Type> mNodes = new Dictionary<string, Type>();

		/// <summary>
        /// Represent single XML tag.
        /// </summary>
		[Serializable]
		public class Node {
			private List<Node> mChildren = new List<Node>();

			/// <summary>
            /// Parent Node;
            /// </summary>
			public Node Parent { get; set; }

			/// <summary>
            /// Set of children node.
            /// </summary>
			public IEnumerable<Node> Children { get { return mChildren; } set { SetChildren(value); } }

			/// <summary>
            /// Value of the Node.
            /// </summary>
			public string Value { get; set; }

			/// <summary>
            /// Key value used in search via 'Find' method.
            /// </summary>
            /// <returns></returns>
			protected virtual string GetKey() { return string.Empty; }

			/// <summary>
            /// Recursively searches for nodes of the given Type in children.
            /// </summary>
            /// <typeparam name="T">Type of node to search for.</typeparam>
            /// <returns>Set of discovered nodes of the given type.</returns>
			public IEnumerable<T> Find<T>() where T : Node {
				var result = new List<T>();

				var children = Children.Where(c => c is T).Select(c => c as T);
				if(children != null && children.Any())
					result.AddRange(children);

				foreach(var c in Children) {
					children = c.Find<T>();
					if(children != null && children.Any())
						result.AddRange(children);
				}

				return result;
			}

			/// <summary>
			/// Recursively searches for nodes of the given Type and 'key' in children.
			/// </summary>
			/// <typeparam name="T">Type of node to search for.</typeparam>
			/// <param name="key">Value of key which will be used in search.</param>
			/// <returns>Set of discovered nodes of the given type and key.</returns>
			public IEnumerable<T> Find<T>(string key) where T : Node {
				var result = new List<T>();

				var children = Children.Where(c => c is T).Select(c => c as T).Where(c => c.GetKey() == key);
				if(children != null && children.Any())
					result.AddRange(children);

				foreach(var c in Children) {
					children = c.Find<T>(key);
					if(children != null && children.Any())
						result.AddRange(children);
				}

				return result;
			}

			/// <summary>
            /// Adds child node.
            /// </summary>
            /// <param name="node">Child node to add.</param>
			public void Add(Node node) {
				node.Parent = this;
				mChildren.Add(node);
			}

			/// <summary>
			/// Adds list of children nodes.
			/// </summary>
			/// <param name="nodes">Children nodes to add.</param>
			public void Add(IEnumerable<Node> nodes) {
				foreach(var n in nodes)
					n.Parent = this;
				mChildren.AddRange(nodes);
			}

			/// <summary>
            /// Deletes nodes from children set.
            /// </summary>
            /// <param name="node">Node to delete.</param>
			public void Remove(Node node) {
				mChildren.Remove(node);
			}

			/// <summary>
			/// Deletes nodes of the given type from children set.
			/// </summary>
			/// <typeparam name="T">Type of node to delete.</typeparam>
			public void Remove<T>() where T : Node {
				Children = Children.Where(c => !(c is T));
			}

			private void SetChildren(IEnumerable<Node> children) {
				mChildren = children.ToList();
				for(int i = 0; i < mChildren.Count; i++) {
					mChildren[i].Parent = this;
				}
			}
		}

		/// <summary>
		/// Registers type which could be deserialized from XML document.
		/// </summary>
		/// <param name="node">Type of node.</param>
		/// <param name="name">Name of the corresponded XML tag.</param>
		/// <returns>Instance of XML deserializer.</returns>
		public DeserializerXml Register(Type node, string name) {
			if(typeof(Node).IsAssignableFrom(node)) {
				mNodes[name] = node;
			} else {
				Console.WriteLine($"[ParserXML.Register] Not possible to register type '{node.Name}' because it must be inherited from 'Node' type!");
			}
			return this;
		}

		/// <summary>
		/// Registers type which could be deserialized from XML document.
		/// </summary>
		/// <typeparam name="T">Type of node.</typeparam>
		/// <param name="name">Name of the corresponded XML tag.</param>
		/// <returns>Instance of XML deserializer.</returns>
		public DeserializerXml Register<T>(string name) where T : Node, new() {
			return Register(typeof(T), name);
		}

		/// <summary>
		/// Performs data deserialization from the given XML document.
		/// </summary>
		/// <param name="text">Text which contains XML document.</param>
		/// <returns>Node of Deserialized data.</returns>
		public Node Deserialize(string text) {
			var parser = new ParserXml();
			parser.Load(text);
			if(parser.Root != null) {
				var instance = CreateInstance(parser.Root);
				return instance;
			}
			return null;
		}

		/// <summary>
		/// Performs data deserialization from the given XML document.
		/// </summary>
		/// <typeparam name="T">Type to which data must be deserialized.</typeparam>
		/// <param name="text">Text which contains XML document.</param>
		/// <returns>Node of Deserialized data.</returns>
		public T Deserialize<T>(string text) where T : Node {
			return Deserialize(text) as T;
		}

		private Type FindType(string name) {
            if(mNodes.TryGetValue(name, out Type type)) {
                return type;
            }
            return null;
		}

		private Node CreateInstance(ParserXml.Tag tag) {
			var type = FindType(tag.Name);
			if(type != null) {
                if(Activator.CreateInstance(type) is Node instance) {
                    instance.Bind(tag.Attributes.ToDictionary(a => a.Name, a => a.Value, StringComparer.OrdinalIgnoreCase));

					var childrenNodes = new List<Node>();
					foreach(var c in tag.Children) {
						var cNode = CreateInstance(c);
						if(cNode != null) {
							childrenNodes.Add(cNode);
						}
					}

					instance.Children = childrenNodes;
					instance.Value = tag.Value;
					return instance;
                }
            }
			return null;
		}
    }
}