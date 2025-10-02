using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Drawing.Imaging;



namespace Assignment4
{
     public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        
        private float rotationAngle, scaleFactor;
        private bool scalingUp;
        
        private int modelLoc, viewLoc, projLoc;

        private int _texture;
        private TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        private TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear;
        private TextureMagFilter magFilter = TextureMagFilter.Linear;
        
        
        
        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(1280, 768));
            
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the OpenGL viewport to match the new window dimensions
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();
                
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            
            GL.ClearColor(new Color4(0.318f, 0.592f,0.804f,1.0f));

            
            float[] vertices = {
                // positions          // texture coords
                // Front face
                -0.5f, -0.5f,  0.5f,   0.0f, 0.0f, // bottom-left
                0.5f, -0.5f,  0.5f,   1.0f, 0.0f, // bottom-right
                0.5f,  0.5f,  0.5f,   1.0f, 1.0f, // top-right
                -0.5f,  0.5f,  0.5f,   0.0f, 1.0f, // top-left

                // Back face
                -0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
                0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
                0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,   1.0f, 1.0f,

                // Left face
                -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,

                // Right face
                0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
                0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
                0.5f,  0.5f,  0.5f,   0.0f, 1.0f,
                0.5f,  0.5f, -0.5f,   1.0f, 1.0f,

                // Top face
                -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
                0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
                0.5f,  0.5f,  0.5f,   1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,   0.0f, 0.0f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
                0.5f, -0.5f,  0.5f,   1.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,   0.0f, 1.0f
            };
            
            uint[] indices = {
                0, 1, 2,   2, 3, 0,       // front
                4, 5, 6,   6, 7, 4,       // back
                8, 9, 10,  10, 11, 8,     // left
                12, 13, 14, 14, 15, 12,   // right
                16, 17, 18, 18, 19, 16,   // top
                20, 21, 22, 22, 23, 20    // bottom
            };
            
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexArrayHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Vertex shader with model, view, projection matrices
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout (location = 1) in vec2 aTexCoord;
                
                uniform mat4 uModel;
                uniform mat4 uView;
                uniform mat4 uProj;
                out vec2 TexCoord;

                void main()
                {
                    gl_Position = uProj * uView * uModel * vec4(aPosition, 1.0);
                    TexCoord = aTexCoord; 
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                out vec4 FragColor;
                in vec2 TexCoord;
    
                uniform sampler2D ourTexture;

                void main()
                {
                    FragColor = texture(ourTexture, TexCoord);
                }
            ";
            
            // Compile shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            // Create shader program and link shaders
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);
            // CheckProgram(shaderProgramHandle);
            
            
            // Cleanup shaders after linking (no longer needed individually)
            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            
            
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string texturePath = Path.Combine(baseDir, "Assets", "wall.jpg");
            _texture = LoadTexture(texturePath);
            
            
            // Get uniform locations
            modelLoc = GL.GetUniformLocation(shaderProgramHandle, "uModel");
            viewLoc = GL.GetUniformLocation(shaderProgramHandle, "uView");
            projLoc = GL.GetUniformLocation(shaderProgramHandle, "uProj");
            
            // Initialize transformation array
            rotationAngle =  0.5f; // stagger initial rotations
            scaleFactor = 1f;
            scalingUp = true;
            
        }

        protected override void OnUnload()
        {
            // Unbind and delete buffers and shader program
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            
            // Rotate continuously
            rotationAngle+= (float)args.Time; // different speed for each triangle

            // Oscillating scale between 0.5 and 1.5
            if (scalingUp)
            {
                scaleFactor += (float)args.Time;
                if (scaleFactor >= 1.5f) scalingUp = false;
            }
            else
            {
                scaleFactor -= (float)args.Time;
                if (scaleFactor <= 0.5f) scalingUp = true;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            
            // Clear the screen with background color
            // GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            
            // Use our shader program
            GL.UseProgram(shaderProgramHandle);

            // View matrix (camera looking at origin)
            Matrix4 view = Matrix4.LookAt(
                new Vector3(2.5f, 2.5f, 2.5f),
                Vector3.Zero,
                Vector3.UnitY);

            // Projection matrix (perspective)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                (float)Size.X / Size.Y,
                0.1f,
                100f
            );

            // Send view and projection to shader (same for all triangles)
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);
            
            
            // Bind the VAO
            GL.BindVertexArray(vertexArrayHandle);
            
            // Rotation quaternion for this triangle
            Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, rotationAngle);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);

            // Scaling
            Matrix4 scaleMatrix = Matrix4.CreateScale(scaleFactor);

            // Translation: spread triangles along X axis
            Matrix4 translationMatrix = Matrix4.CreateTranslation(-2f + 1 * 2f, 0f, 0f);

            // Combine transformations: Model = Translation * Rotation * Scale
            Matrix4 model = scaleMatrix * rotationMatrix * translationMatrix;

            // Send model matrix to shader
            GL.UniformMatrix4(modelLoc, false, ref model);
            
            // draw the cube
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            // Display the rendered frame
            SwapBuffers();
        }
        
        // Helper function to check for shader compilation errors
        private void CheckShaderCompile(int shaderHandle, string shaderName)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
            }
        }
        
        private void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) throw new Exception(GL.GetProgramInfoLog(program));
        }
        
        private int LoadTexture(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"Could not find texture file: {path}");

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            // Initial wrap and filter
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            using (Bitmap bmp = new Bitmap(path))
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                var data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb); // fully qualified

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return texId;
        }
    }

}