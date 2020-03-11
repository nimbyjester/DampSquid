//using System;
//using System.Collections.Generic;
//using System.IO;
//using Veldrid;
//using Veldrid.ImageSharp;
//using Veldrid.SPIRV;

//namespace DampEngine
//{
//    internal static class StaticResourceCache
//    {
//        private static readonly Dictionary<GraphicsPipelineDescription, Pipeline> s_pipelines
//            = new Dictionary<GraphicsPipelineDescription, Pipeline>();

//        private static readonly Dictionary<ResourceLayoutDescription, ResourceLayout> s_layouts
//            = new Dictionary<ResourceLayoutDescription, ResourceLayout>();

//        private static readonly Dictionary<ShaderSetCacheKey, (Shader, Shader)> s_shaderSets
//            = new Dictionary<ShaderSetCacheKey, (Shader, Shader)>();

//        private static readonly Dictionary<ImageSharpTexture, Texture> s_textures
//            = new Dictionary<ImageSharpTexture, Texture>();

//        private static readonly Dictionary<Texture, TextureView> s_textureViews = new Dictionary<Texture, TextureView>();

//        private static readonly Dictionary<ResourceSetDescription, ResourceSet> s_resourceSets
//            = new Dictionary<ResourceSetDescription, ResourceSet>();

//        private static Texture _pinkTex;

//        public static readonly ResourceLayoutDescription ProjViewLayoutDescription = new ResourceLayoutDescription(
//            new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
//            new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex));

//        public static Pipeline GetPipeline(ResourceFactory factory, ref GraphicsPipelineDescription desc)
//        {
//            if (!s_pipelines.TryGetValue(desc, out Pipeline p))
//            {
//                p = factory.CreateGraphicsPipeline(ref desc);
//                s_pipelines.Add(desc, p);
//            }

//            return p;
//        }

//        public static ResourceLayout GetResourceLayout(ResourceFactory factory, ResourceLayoutDescription desc)
//        {
//            if (!s_layouts.TryGetValue(desc, out ResourceLayout p))
//            {
//                p = factory.CreateResourceLayout(ref desc);
//                s_layouts.Add(desc, p);
//            }

//            return p;
//        }

//        public static (Shader vs, Shader fs) GetShaders(
//            GraphicsDevice gd,
//            ResourceFactory factory,
//            string name)
//        {
//            SpecializationConstant[] constants = ShaderHelper.GetSpecializations(gd);
//            ShaderSetCacheKey cacheKey = new ShaderSetCacheKey(name, constants);
//            if (!s_shaderSets.TryGetValue(cacheKey, out (Shader vs, Shader fs) set))
//            {
//                set = ShaderHelper.LoadSPIRV(gd, factory, name);
//                s_shaderSets.Add(cacheKey, set);
//            }

//            return set;
//        }

//        public static void DestroyAllDeviceObjects()
//        {
//            foreach (KeyValuePair<GraphicsPipelineDescription, Pipeline> kvp in s_pipelines)
//            {
//                kvp.Value.Dispose();
//            }
//            s_pipelines.Clear();

//            foreach (KeyValuePair<ResourceLayoutDescription, ResourceLayout> kvp in s_layouts)
//            {
//                kvp.Value.Dispose();
//            }
//            s_layouts.Clear();

//            foreach (KeyValuePair<ShaderSetCacheKey, (Shader, Shader)> kvp in s_shaderSets)
//            {
//                kvp.Value.Item1.Dispose();
//                kvp.Value.Item2.Dispose();
//            }
//            s_shaderSets.Clear();

//            foreach (KeyValuePair<ImageSharpTexture, Texture> kvp in s_textures)
//            {
//                kvp.Value.Dispose();
//            }
//            s_textures.Clear();

//            foreach (KeyValuePair<Texture, TextureView> kvp in s_textureViews)
//            {
//                kvp.Value.Dispose();
//            }
//            s_textureViews.Clear();

//            _pinkTex?.Dispose();
//            _pinkTex = null;

//            foreach (KeyValuePair<ResourceSetDescription, ResourceSet> kvp in s_resourceSets)
//            {
//                kvp.Value.Dispose();
//            }
//            s_resourceSets.Clear();
//        }

//        internal static Texture GetTexture2D(GraphicsDevice gd, ResourceFactory factory, ImageSharpTexture textureData)
//        {
//            if (!s_textures.TryGetValue(textureData, out Texture tex))
//            {
//                tex = textureData.CreateDeviceTexture(gd, factory);
//                s_textures.Add(textureData, tex);
//            }

//            return tex;
//        }

//        internal static TextureView GetTextureView(ResourceFactory factory, Texture texture)
//        {
//            if (!s_textureViews.TryGetValue(texture, out TextureView view))
//            {
//                view = factory.CreateTextureView(texture);
//                s_textureViews.Add(texture, view);
//            }

//            return view;
//        }

//        internal static ResourceSet GetResourceSet(ResourceFactory factory, ResourceSetDescription description)
//        {
//            if (!s_resourceSets.TryGetValue(description, out ResourceSet ret))
//            {
//                ret = factory.CreateResourceSet(ref description);
//                s_resourceSets.Add(description, ret);
//            }

//            return ret;
//        }
//    }

//    public struct ShaderSetCacheKey : IEquatable<ShaderSetCacheKey>
//    {
//        public string Name { get; }
//        public SpecializationConstant[] Specializations { get; }

//        public ShaderSetCacheKey(string name, SpecializationConstant[] specializations) : this()
//        {
//            Name = name;
//            Specializations = specializations;
//        }

//        public bool Equals(ShaderSetCacheKey other)
//        {
//            return Name.Equals(other.Name) && ArraysEqual(Specializations, other.Specializations);
//        }

//        public override int GetHashCode()
//        {
//            int hash = Name.GetHashCode();
//            foreach (var specConst in Specializations)
//            {
//                hash ^= specConst.GetHashCode();
//            }
//            return hash;
//        }

//        private bool ArraysEqual<T>(T[] a, T[] b) where T : IEquatable<T>
//        {
//            if (a.Length != b.Length) { return false; }

//            for (int i = 0; i < a.Length; i++)
//            {
//                if (!a[i].Equals(b[i])) { return false; }
//            }

//            return true;
//        }
//    }

//    public static class ShaderHelper
//    {
//        public static (Shader vs, Shader fs) LoadSPIRV(
//            GraphicsDevice gd,
//            ResourceFactory factory,
//            string setName)
//        {
//            byte[] vsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Vertex);
//            byte[] fsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Fragment);
//            bool debug = false;
//#if DEBUG
//            debug = true;
//#endif
//            Shader[] shaders = factory.CreateFromSpirv(
//                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", debug),
//                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", debug),
//                GetOptions(gd));

//            Shader vs = shaders[0];
//            Shader fs = shaders[1];

//            vs.Name = setName + "-Vertex";
//            fs.Name = setName + "-Fragment";

//            return (vs, fs);
//        }

//        private static CrossCompileOptions GetOptions(GraphicsDevice gd)
//        {
//            SpecializationConstant[] specializations = GetSpecializations(gd);

//            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
//                && !gd.IsDepthRangeZeroToOne;
//            bool invertY = false;

//            return new CrossCompileOptions(fixClipZ, invertY, specializations);
//        }

//        public static SpecializationConstant[] GetSpecializations(GraphicsDevice gd)
//        {
//            bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

//            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
//            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
//            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
//            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));

//            PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
//            bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
//                || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
//            specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

//            return specializations.ToArray();
//        }

//        public static byte[] LoadBytecode(GraphicsBackend backend, string setName, ShaderStages stage)
//        {
//            string stageExt = stage == ShaderStages.Vertex ? "vert" : "frag";
//            string name = setName + "." + stageExt;

//            if (backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.Direct3D11)
//            {
//                string bytecodeExtension = GetBytecodeExtension(backend);
//                string bytecodePath = AssetHelper.GetPath(Path.Combine("Shaders", name + bytecodeExtension));
//                if (File.Exists(bytecodePath))
//                {
//                    return File.ReadAllBytes(bytecodePath);
//                }
//            }

//            string extension = GetSourceExtension(backend);
//            string path = AssetHelper.GetPath(Path.Combine("Shaders.Generated", name + extension));
//            return File.ReadAllBytes(path);
//        }

//        private static string GetBytecodeExtension(GraphicsBackend backend)
//        {
//            switch (backend)
//            {
//                case GraphicsBackend.Direct3D11: return ".hlsl.bytes";
//                case GraphicsBackend.Vulkan: return ".spv";
//                case GraphicsBackend.OpenGL:
//                    throw new InvalidOperationException("OpenGL and OpenGLES do not support shader bytecode.");
//                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
//            }
//        }

//        private static string GetSourceExtension(GraphicsBackend backend)
//        {
//            switch (backend)
//            {
//                case GraphicsBackend.Direct3D11: return ".hlsl";
//                case GraphicsBackend.Vulkan: return ".450.glsl";
//                case GraphicsBackend.OpenGL:
//                    return ".330.glsl";
//                case GraphicsBackend.OpenGLES:
//                    return ".300.glsles";
//                case GraphicsBackend.Metal:
//                    return ".metallib";
//                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
//            }
//        }
//    }

//    internal static class AssetHelper
//    {
//        private static readonly string s_assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");

//        internal static string GetPath(string assetPath)
//        {
//            return Path.Combine(s_assetRoot, assetPath);
//        }
//    }
//}
