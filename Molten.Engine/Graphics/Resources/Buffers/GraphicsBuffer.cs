namespace Molten.Graphics
{
    public abstract class GraphicsBuffer : GraphicsResource
    {
        protected GraphicsBuffer(GraphicsDevice device, uint stride, uint numElements, GraphicsResourceFlags flags, GraphicsBufferType type) :
            base(device, flags)
        {
            BufferType = type;
            Stride = stride;
            ElementCount = numElements;
            SizeInBytes = stride * numElements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="data"></param>
        /// <param name="staging"></param>
        /// <param name="completeCallback"></param>
        public void SetData<T>(GraphicsPriority priority, T[] data, bool discard, Action completeCallback = null)
    where T : unmanaged
        {
            SetData(priority, data, 0, (uint)data.Length, discard, 0, completeCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="data"></param>
        /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
        /// <param name="elementCount"></param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        /// <param name="staging"></param>
        /// <param name="completeCallback"></param>
        /// <param name="discard">If true, the previous data will be discarded. Ignored if not applicable to the current buffer.</param>
        public void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0, Action completeCallback = null)
            where T : unmanaged
        {
            BufferSetTask<T> op = new BufferSetTask<T>()
            {
                ByteOffset = byteOffset,
                CompletionCallback = completeCallback,
                DestBuffer = this,
                MapType = discard ? GraphicsMapType.Discard : GraphicsMapType.Write,
                ElementCount = elementCount,
            };

            // Custom handling of immediate command, so that we potentially avoid a data copy.
            if (priority == GraphicsPriority.Immediate)
            {
                op.Data = data;
                op.DataStartIndex = startIndex;
                op.Process(Device.Queue, this);
            }
            else
            {
                // Only copy the part we need from the source data, starting from startIndex.
                op.Data = new T[data.Length];
                op.DataStartIndex = 0;
                Array.Copy(data, (int)startIndex, op.Data, 0, elementCount);
                QueueTask(priority, op);
            }
        }

        /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="priority">The priority of the operation</param>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        /// <param name="completionCallback">A callback to run once the operation is completed.</param>
        public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback = null)
            where T : unmanaged
        {
            if (!Flags.Has(GraphicsResourceFlags.CpuRead))
                throw new GraphicsResourceException(this, "Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            QueueTask(priority, new BufferGetTask<T>()
            {
                ByteOffset = byteOffset,
                DestArray = destination,
                DestIndex = startIndex,
                Count = count,
                MapType = GraphicsMapType.Read,
                CompletionCallback = completionCallback,
            });
        }

        /// <summary>Copies all the data in the current <see cref="GraphicsBuffer"/> to the destination <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="priority">The priority of the operation</param>
        /// <param name="destination">The <see cref="GraphicsBuffer"/> to copy to.</param>
        /// <param name="sourceRegion"></param>
        /// <param name="destByteOffset"></param>
        /// <param name="completionCallback">A callback to invoke once the operation is completed.</param>
        public void CopyTo(GraphicsPriority priority, GraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0,
            Action<GraphicsResource> completionCallback = null)
        {
            QueueTask(priority, new SubResourceCopyTask()
            {
                CompletionCallback = completionCallback,
                DestResource = destination,
                DestStart = new Vector3UI(destByteOffset, 0, 0),
                SrcRegion = sourceRegion,
            });
        }

        public void GetStream(GraphicsPriority priority, GraphicsMapType mapType, Action<GraphicsBuffer, GraphicsStream> callback, GraphicsBuffer staging = null)
        {
            QueueTask(priority, new BufferGetStreamTask()
            {
                ByteOffset = 0,
                Staging = staging,
                StreamCallback = callback,
                MapType = mapType,
            });
        }

        /// <summary>
        /// Returns the correct <see cref="GraphicsConstantVariable"/> based on the provided <see cref="ConstantBufferVariableInfo"/>.
        /// </summary>
        /// <param name="vInfo"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected unsafe GraphicsConstantVariable CreateConstantVariable(ConstantBufferVariableInfo vInfo, string name)
        {
            if(!(this is IConstantBuffer parent))
                throw new GraphicsResourceException(this, "The current GraphicsBuffer must implement IConstantBuffer.");

            uint columns = vInfo.Type.ColumnCount;
            uint rows = vInfo.Type.RowCount;
            uint elementCount = vInfo.Type.Elements;

            switch (vInfo.Type.Class)
            {
                default:
                    if (elementCount > 0)
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarArray<int>(parent, elementCount, name);
                            case ShaderVariableType.UInt:
                                return new ScalarArray<uint>(parent, elementCount, name);
                            case ShaderVariableType.Float:
                                return new ScalarArray<float>(parent, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Int:
                                return new ScalarVariable<int>(parent, rows, columns, name);
                            case ShaderVariableType.UInt:
                                return new ScalarVariable<uint>(parent, rows, columns, name);
                            case ShaderVariableType.Float:
                                return new ScalarVariable<float>(parent, rows, columns, name);
                        }
                    }
                    break;

                case ShaderVariableClass.MatrixColumns:
                    if (elementCount > 0)
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
                                if (columns == 4 && rows == 4)
                                    return new ScalarFloat4x4ArrayVariable(parent, elementCount, name);
                                else if (columns == 3 && rows == 3)
                                    return new ScalarFloat3x3ArrayVariable(parent, elementCount, name);
                                else
                                    return new ScalarMatrixArray<float>(parent, rows, columns, elementCount, name);

                            default:
                                return new ScalarMatrixArray<float>(parent, rows, columns, elementCount, name);
                        }
                    }
                    else
                    {
                        switch (vInfo.Type.Type)
                        {
                            case ShaderVariableType.Float:
                                if (vInfo.Type.ColumnCount == 4 && rows == 4)
                                    return new ScalarFloat4x4Variable(parent, name);
                                else if (vInfo.Type.ColumnCount == 2 && rows == 3)
                                    return new ScalarFloat3x2Variable(parent, name);
                                else if (vInfo.Type.ColumnCount == 3 && rows == 3)
                                    return new ScalarFloat3x3Variable(parent, name);
                                else
                                    return new ScalarVariable<float>(parent, rows, columns, name);

                            default:
                                return new ScalarVariable<float>(parent, rows, columns, name);
                        }
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets the stride (byte size) of each element within the current <see cref="GraphicsBuffer"/>.
        /// </summary>
        public uint Stride { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="GraphicsBuffer"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>
        /// Gets the total size of the buffer, in bytes.
        /// </summary>
        public override uint SizeInBytes { get; protected set; }

        /// <summary>
        /// Gets the type of the current <see cref="GraphicsBuffer"/>.
        /// </summary>
        public GraphicsBufferType BufferType { get; }

        /// <summary>
        /// Gets the vertex format of the current <see cref="GraphicsBuffer"/>, if any.
        /// <para>This property is only set if the current <see cref="BufferType"/> is <see cref="GraphicsBufferType.Vertex"/>.</para>
        /// </summary>
        public VertexFormat VertexFormat { get; internal set; }
    }
}
