using System;
using System.Collections.Generic;

namespace Classroom.Models;

public partial class MaterialAttachment
{
    public int AttachmentId { get; set; }

    public int MaterialId { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string? Thumbnail { get; set; }

    public virtual Material Material { get; set; } = null!;
}
