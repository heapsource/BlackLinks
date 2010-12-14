
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace BlackLinks.Templates
{
	public sealed class CSharpTemplateCompiler : TemplateCompiler
	{
		 const string globalNamespace = "RenderedTemplates";
		internal CodeDomProvider csharp = null;
		CodeNamespace ns = new CodeNamespace (globalNamespace);
		CodeCompileUnit codeCompilerUnit = null;
		public CSharpTemplateCompiler ()
		{
			csharp = CodeDomProvider.CreateProvider ("csharp");
			codeCompilerUnit = new CodeCompileUnit ();
			codeCompilerUnit.Namespaces.Add (ns);
		}
		List<GeneratedResourceClass> generatedResourcesClassNames = new List<GeneratedResourceClass>();
		
		class GeneratedResourceClass
		{
			public string DiscoveryPath;
			public string ClassName;
		}
		
		protected override TemplatesCompilationResult OnCompile ()
		{
			TemplatesCompilationResult result = new TemplatesCompilationResult ();
			foreach (var source in this.Sources)
			{
				using (StreamReader reader = new StreamReader (source.SourceStream))
				{
					InternalCSharpTemplateCompiler compiler = new InternalCSharpTemplateCompiler (reader, ns,source.ReferenceFilePath);
					compiler.generate ();
					generatedResourcesClassNames.Add(new GeneratedResourceClass
						{
						ClassName =
						compiler.className,
							DiscoveryPath = source.DiscoveryPath
					});
				}
			}
			generateDiscoveryClass();
			#if DEBUG
			using (FileStream fs = new FileStream ("/home/thepumpkin/template.cs", FileMode.Create, FileAccess.Write)) {
				using (StreamWriter writer = new StreamWriter (fs)) {
					csharp.GenerateCodeFromCompileUnit (this.codeCompilerUnit, writer, null);
				}
			}
			#endif
			string[] assemblies = new string[] { "BlackLinks" };
			
			CompilerParameters prms = new CompilerParameters (assemblies);
			if (!string.IsNullOrEmpty (this.OutputAssemblyPath)) {
				prms.OutputAssembly = this.OutputAssemblyPath;
			}
			CompilerResults results = this.csharp.CompileAssemblyFromDom (prms, new CodeCompileUnit[] { this.codeCompilerUnit });
			
			Console.WriteLine ("Errors Count= {0}", results.Errors.Count);
			if(results.CompiledAssembly != null)
			{
				result.Assemblies.Add(results.CompiledAssembly);
			}
			foreach (CompilerError error in results.Errors) {
#if DEBUG
				Console.WriteLine ("\t{0} at file {1}#{2}", error.ErrorText, error.FileName, error.Line);
#endif
				
				result.CompilationErrors.Add(error);
			}
			
			return result;
		}
		
		void generateDiscoveryClass ()
		{
			CodeTypeDeclaration discoveryClass = new CodeTypeDeclaration ("DiscoveryProvider");
			discoveryClass.BaseTypes.Add (typeof(ITemplateInstanceDiscoveryProvider));
			ns.Types.Add (discoveryClass);
			
			CodeMemberMethod discoverInstanceMethod = new CodeMemberMethod ();
			discoverInstanceMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "path"));
			discoverInstanceMethod.Name = "DiscoverInstance";
			discoverInstanceMethod.ReturnType = new CodeTypeReference (typeof(TemplateRenderResource));
			discoverInstanceMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			discoveryClass.Members.Add (discoverInstanceMethod);
			
			foreach (GeneratedResourceClass generatedClass in this.generatedResourcesClassNames)
			{
				CodeConditionStatement condition = new CodeConditionStatement ();
				condition.Condition = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("path"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (generatedClass.DiscoveryPath));
				condition.TrueStatements.Add (new CodeMethodReturnStatement (new CodeObjectCreateExpression (
						getResourceFullTypeName (generatedClass.ClassName))));
				discoverInstanceMethod.Statements.Add (condition);
			}
			discoverInstanceMethod.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (null)));
		
			// Generate Discovery Attribute
			
			this.codeCompilerUnit.AssemblyCustomAttributes.Add (
				new CodeAttributeDeclaration (
					new CodeTypeReference (typeof(TemplateInstanceDiscoveryProviderAttribute)),
				 new CodeAttributeArgument[] { 
				new CodeAttributeArgument ("DiscoveryType",
				new CodeTypeOfExpression (getResourceFullTypeName (discoveryClass.Name)))}));
		}
		
		public string getResourceFullTypeName(string className)
		{
			return string.Format("{0}.{1}",globalNamespace,className);
		}
	}
	class TemplateBlock
	{
		const string CodeStartTag ="<$";
		const string CodeEndTag = "$>";
		public bool isLineOfText;
		public long Line;
		public BlockType Type;
		public StringBuilder Buffer = new StringBuilder();
		public bool IsCodeEnd
		{
			get 
			{
				return endsWith(CodeEndTag);
			}
		}
		
		bool endsWith (string str)
		{
			int bufferLength = this.Buffer.Length;
			if (bufferLength < str.Length)
			{
				return false;
			}
			for (int i = 0; i < str.Length; i++)
			{
				int bi = bufferLength - str.Length + i;
				if(str[i] != this.Buffer[bi])return false;
			}
			return true;
		}
		
		public bool IsCodeStart {
			get { return endsWith (CodeStartTag); }
		}
		public void RemoveCodeStart ()
		{
			Buffer.Remove (Buffer.Length - CodeStartTag.Length, CodeStartTag.Length);
		}
		public void RemoveCodeEnd ()
		{
			Buffer.Remove (Buffer.Length - CodeEndTag.Length, CodeEndTag.Length);
		}
		public bool Empty {
			get {
				return Buffer.Length == 0;
			}
		}
		public bool IsCodeExpression
		{
			get 
			{
				return this.Buffer.Length > 0 && this.Buffer[0] == '=';
			}
		}
		public string CodeSnippet
		{
			get 
			{
				if(IsCodeExpression)
				{
					StringBuilder sb = new StringBuilder(this.Buffer.ToString());
					sb.Remove(0,1);
					return sb.ToString();
				}
				return this.Buffer.ToString();
			}
		}
	}
	enum BlockType
	{
		Literal,
		Code
	}
	class InternalCSharpTemplateCompiler
	{
		TemplatesCompilationParameters templateCompilationParameters;
		StreamReader reader;
		List<TemplateBlock> blocks = new List<TemplateBlock>();
	
		CodeTypeDeclaration templateResourceClass = null;
		string referenceFileName;
		public string className = null;
		public InternalCSharpTemplateCompiler ( StreamReader reader, CodeNamespace ns, string referenceFileName)
		{
			this.referenceFileName = referenceFileName;
			this.reader = reader;
			this.className = "Resource" + this.GetHashCode ().ToString ().Replace("-",string.Empty);
			templateResourceClass = new CodeTypeDeclaration (this.className);
			ns.Types.Add (templateResourceClass);
			
		}
		TemplateBlock currentBlock;
		string currentLine;
		long lineCount = 1;
		public void generate ()
		{
			parseBLocks ();
#if DEBUG
			foreach (TemplateBlock block in blocks)
			{
				Console.WriteLine ("Block {0} at line {1} = '{2}'",block.Type,block.Line,block.Buffer.ToString());
			}
#endif
			generateAndCompile();
		}
		void generateAndCompile ()
		{
			templateResourceClass.BaseTypes.Add (typeof(TemplateRenderResource));
			templateResourceClass.IsClass = true;
			CodeMemberMethod renderMethod = new CodeMemberMethod ();
			renderMethod.Name = "Render";
			renderMethod.Attributes = MemberAttributes.Override | MemberAttributes.Family;
			templateResourceClass.Members.Add (renderMethod);
			
			foreach (TemplateBlock block in this.blocks)
			{
				CodeStatement statement = null;
				if (block.Type == BlockType.Literal)
				{
					string bufferText = block.Buffer.ToString ();
					
						statement = new CodeExpressionStatement (new CodeMethodInvokeExpression (new CodeThisReferenceExpression (), block.isLineOfText ? "WriteLine" : "Write",
					new CodePrimitiveExpression (bufferText)));
				
				} else if (block.Type == BlockType.Code)
				{
					string snippet = block.CodeSnippet;
					if (snippet.Length != 0)
					{
						if (block.IsCodeExpression)
						{
							CodeMethodInvokeExpression exp = new CodeMethodInvokeExpression (new CodeThisReferenceExpression (), "Write", new CodeSnippetExpression (snippet));
							statement = new CodeExpressionStatement (exp);
						}
						else
						{
							statement = new CodeSnippetStatement (snippet);
						}
					}
				}
				if (statement != null)
				{
					var codePragma = new CodeLinePragma (this.referenceFileName, (int)block.Line);
					statement.LinePragma = codePragma;
					renderMethod.Statements.Add (statement);
				}
			}
		}
		void parseBLocks ()
		{
			continueWithBlocks ();
			while ((currentLine = reader.ReadLine ()) != null) {
				parseLine ();
				lineCount++;
				if (currentBlock.Type == BlockType.Literal)
				{
					continueWithBlocks();
				}
			}
			continueWithBlocks ();
		}
		void continueWithBlocks ()
		{
			if (currentBlock != null && !currentBlock.Empty)
			{
				this.blocks.Add (currentBlock);
			}
			currentBlock = new TemplateBlock ();
			currentBlock.Line = lineCount;
		}
		void parseLine ()
		{
			foreach (char c in currentLine)
			{
				currentBlock.Buffer.Append (c);
				if (currentBlock.IsCodeStart)
				{
					currentBlock.RemoveCodeStart ();
					continueWithBlocks ();
					currentBlock.Type = BlockType.Code;
				} else if (currentBlock.IsCodeEnd)
				{
					currentBlock.RemoveCodeEnd ();
					continueWithBlocks ();
				}
			}
			if (currentBlock.Type == BlockType.Literal)
			{
				currentBlock.isLineOfText = true;
			}
		}
	}
}

