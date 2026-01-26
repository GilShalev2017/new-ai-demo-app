using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    public class BoundingBoxDetection
    {
        public BoundingBoxObjectType ObjectType { get; set; } = BoundingBoxObjectType.GenericObject;
        public double WidthPercentage { get; set; }
        public double HeightPercentage { get; set; }
        public double xCoordinatePercentage { get; set; }
        public double yCoordinatePercentage { get; set; }
        //public ulong? FrameIndex { get; set; }
        public ulong VideoTimeOffsetMillis { get; set; }
        public int? SubjectID { get; set; }
        public double DurationMillis { get; set; }
        public string Description { get; set; } = "";
        public string AIEngine { get; set; } = null!;
        /// <summary>
        ///  Json encoded result ;the object will be specific to each AI Engine.
        /// </summary>
        public AIEngineResult? AIEngineResult { get; set; }
        public string? ImageDataBase64 { get; set; }  // Base64 for JSON transport
        public double Confidence { get; set; } = 0;
    }

    public class BoundingBox
    {
        public float Top { get; set; } = 0f;
        public float Left { get; set; } = 0f;
        public float Right { get; set; } = 0f;
        public float Bottom { get; set; } = 0f;
    }

    //used for: FaceDetection, ObjectDetection 

    public enum BoundingBoxObjectType 
    {
        Face,
        LicensePlate,
        GenericObject
    }

    [BsonIgnoreExtraElements]
    public class AIEngineResult
    {
        public string? ObjectId { get; set; }
        /// <summary>
        /// Set to true by the AIEngine in case it is a known face/plate/object
        /// </summary>
        public bool Known { get; set; } = false;
    }

    public class BoundingBoxObjectFilter
    {
        public List<int> ChannelIds { get; set; } = new List<int>();

        /// <summary>
        /// when using this values, make sure you always set this field to a Local DateTime object
        /// </summary>
        public DateTime TimestampStart { get; set; } = DateTime.MinValue;
        /// <summary>
        /// when using this values, make sure you always set this field to a Local DateTime object
        /// </summary>
        public DateTime TimestampEnd { get; set; } = DateTime.MinValue;
        public BoundingBoxObjectType? ObjectType { get; set; }
        public int MaxResults { get; set; } = -1; // no limit.
    }
    [BsonIgnoreExtraElements]
    public class BoundingBoxObject
    {
        public int ChannelId { get; set; } = 0;
        public string ChannelDisplayName { get; set; } = "";
        public BoundingBoxObjectType ObjectType { get; set; } = BoundingBoxObjectType.GenericObject;
        public AIEngineResult? AIEngineResult { get; set; }
        public BoundingBox NormalizedBoundingBox { get; set; } = new();
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TimestampStart { get; set; } = DateTime.MinValue;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TimestampEnd { get; set; } = DateTime.MinValue;
        public string Description { get; set; } = "";
        public string Debug { get; set; } = "";
        public OverlayStyle OverlayStyle { get; set; } = new OverlayStyle();
        public double Confidence { get; set; } = 0;
        /// <summary>
        /// Optional small image data of the detected object (e.g., face or license plate) - Base64
        /// </summary>
        public string? ImageDataBase64 { get; set; } = null;
    }

    public class BoundingBoxObjectsListDTO
    {
        public List<BoundingBoxObjectsResult> Results { set; get; } = new();
    }

    /// <summary>
    /// todo - delete this 
    /// </summary>
    public class BoundingBoxObjectsResult
    {
        public List<BoundingBoxObject> Detections { get; set; } = new();
        public int ChannelId { get; set; }
        public DateTime TimestampStart { get; set; } = DateTime.MinValue;
        public DateTime TimestampEnd { get; set; } = DateTime.MinValue;
    }
    public enum OverlayShapeType
    {
        Rectangle = 0,
        CornersOnly,
        FullFrameOverlayTop
    }
    public class OverlayStyle
    {
        public OverlayShapeType ShapeType { get; set; } = OverlayShapeType.FullFrameOverlayTop;
        public string Color { get; set; } = "#311b92";  //hex "#FF0000" or "red"

    }

    public class OverlayShapeTypeDTO
    {
        public List<string> Shapes { get; set; } = new List<string>();
    }
}
