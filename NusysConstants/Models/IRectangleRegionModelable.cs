namespace NusysIntermediate
{
    /// <summary>
    /// Rectangle region model interface used so we can have different classes of regions
    /// i.e. temporary vs. permanent
    /// </summary>
    public interface IRectangleRegionModelable
    {
        /// <summary>
        /// The normalized topleft point of the region
        /// </summary>
        PointModel TopLeftPoint { get; }
        /// <summary>
        /// the normalized width of the region
        /// </summary>
        double Width { get; }
        /// <summary>
        /// the normalized height of the region
        /// </summary>
        double Height { get; }
    }
}
