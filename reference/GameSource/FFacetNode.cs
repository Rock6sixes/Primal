public class FFacetNode : FNode
{
	public FAtlas _atlas;

	public FShader _shader;

	public int _firstFacetIndex = -1;

	public int _numberOfFacetsNeeded;

	public FFacetRenderLayer _renderLayer;

	public FFacetType _facetType;

	public bool _hasInited;

	public virtual int firstFacetIndex => _firstFacetIndex;

	public FShader shader
	{
		get
		{
			return _shader;
		}
		set
		{
			if (_shader != value)
			{
				_shader = value;
				if (_isOnStage)
				{
					_stage.HandleFacetsChanged();
				}
			}
		}
	}

	public FFacetType facetType => _facetType;

	public virtual void Init(FFacetType facetType, FAtlas atlas, int numberOfFacetsNeeded)
	{
		_facetType = facetType;
		_atlas = atlas;
		if (_shader == null)
		{
			_shader = FShader.defaultShader;
		}
		_numberOfFacetsNeeded = numberOfFacetsNeeded;
		_hasInited = true;
	}

	public void UpdateFacets()
	{
		if (_hasInited)
		{
			_stage.renderer.GetFacetRenderLayer(out _renderLayer, out _firstFacetIndex, _facetType, _atlas, _shader, _numberOfFacetsNeeded);
		}
	}

	public virtual void PopulateRenderLayer()
	{
	}

	public override void HandleAddedToStage()
	{
		if (!_isOnStage)
		{
			base.HandleAddedToStage();
			_stage.HandleFacetsChanged();
		}
	}

	public override void HandleRemovedFromStage()
	{
		if (_isOnStage)
		{
			base.HandleRemovedFromStage();
			_stage.HandleFacetsChanged();
		}
	}
}
