using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sce.Atf;

public static class Resources
{
	[ImageResource("Computer.ico")]
	public static readonly string ComputerImage;

	[ImageResource("diskdrive16.png", "diskdrive24.png", "diskdrive32.png")]
	public static readonly string DiskDriveImage;

	[ImageResource("pin_green16.png", "pin_green24.png", "pin_green32.png")]
	public static readonly string PinGreenImage;

	[ImageResource("pin_grey16.png", "pin_grey24.png", "pin_grey32.png")]
	public static readonly string PinGreyImage;

	[ImageResource("Folder.ico")]
	public static readonly string FolderIcon;

	[ImageResource("Folder16.png", "Folder24.png", "Folder32.png")]
	public static readonly string FolderImage;

	[ImageResource("folderOpenReference16.png")]
	public static readonly string ReferenceFolderOpen;

	[ImageResource("folderClosedReference16.png")]
	public static readonly string ReferenceFolderClosed;

	[ImageResource("Document16.png", "Document24.png", "Document32.png")]
	public static readonly string DocumentImage;

	[ImageResource("printer16.png", "printer24.png", "printer32.png")]
	public static readonly string PrinterImage;

	[ImageResource("printer_preferences16.png", "printer_preferences24.png", "printer_preferences32.png")]
	public static readonly string PrinterPreferencesImage;

	[ImageResource("printer_view16.png", "printer_view24.png", "printer_view32.png")]
	public static readonly string PrinterViewImage;

	[ImageResource("Preferences16.png", "Preferences24.png", "Preferences32.png")]
	public static readonly string PreferencesImage;

	[ImageResource("Help16.png", "Help24.png", "Help32.png")]
	public static readonly string HelpImage;

	[ImageResource("Data16.png", "Data24.png", "Data32.png")]
	public static readonly string DataImage;

	[ImageResource("Selection16.png", "Selection24.png", "Selection32.png")]
	public static readonly string SelectionImage;

	[ImageResource("Save16.png", "Save24.png", "Save32.png")]
	public static readonly string SaveImage;

	[ImageResource("SaveAs16.png", "SaveAs24.png", "SaveAs32.png")]
	public static readonly string SaveAsImage;

	[ImageResource("SaveAll16.png", "SaveAll24.png", "SaveAll32.png")]
	public static readonly string SaveAllImage;

	[ImageResource("Undo16.png", "Undo24.png", "Undo32.png")]
	public static readonly string UndoImage;

	[ImageResource("Redo16.png", "Redo24.png", "Redo32.png")]
	public static readonly string RedoImage;

	[ImageResource("Copy16.png", "Copy24.png", "Copy32.png")]
	public static readonly string CopyImage;

	[ImageResource("Cut16.png", "Cut24.png", "Cut32.png")]
	public static readonly string CutImage;

	[ImageResource("Paste16.png", "Paste24.png", "Paste32.png")]
	public static readonly string PasteImage;

	[ImageResource("Delete16.png", "Delete24.png", "Delete32.png")]
	public static readonly string DeleteImage;

	[ImageResource("Group16.png", "Group24.png", "Group32.png")]
	public static readonly string GroupImage;

	[ImageResource("Ungroup16.png", "Ungroup24.png", "Ungroup32.png")]
	public static readonly string UngroupImage;

	[ImageResource("edit_locked16x16.png", "edit_locked24x24.png", "edit_locked32x32.png")]
	public static readonly string LockImage;

	[ImageResource("edit_unlocked16x16.png", "edit_unlocked24x24.png", "edit_unlocked32x32.png")]
	public static readonly string UnlockImage;

	[ImageResource("Find16.png", "Find24.png", "Find32.png")]
	public static readonly string FindImage;

	[ImageResource("ZoomIn16.png", "ZoomIn24.png", "ZoomIn32.png")]
	public static readonly string ZoomInImage;

	[ImageResource("ZoomOut16.png", "ZoomOut24.png", "ZoomOut32.png")]
	public static readonly string ZoomOutImage;

	[ImageResource("FitToSize16.png", "FitToSize24.png", "FitToSize32.png")]
	public static readonly string FitToSizeImage;

	[ImageResource("AlignLefts16.png", "AlignLefts24.png", "AlignLefts32.png")]
	public static readonly string AlignLeftsImage;

	[ImageResource("AlignRights16.png", "AlignRights24.png", "AlignRights32.png")]
	public static readonly string AlignRightsImage;

	[ImageResource("AlignCenters16.png", "AlignCenters24.png", "AlignCenters32.png")]
	public static readonly string AlignCentersImage;

	[ImageResource("AlignTops16.png", "AlignTops24.png", "AlignTops32.png")]
	public static readonly string AlignTopsImage;

	[ImageResource("AlignBottoms16.png", "AlignBottoms24.png", "AlignBottoms32.png")]
	public static readonly string AlignBottomsImage;

	[ImageResource("AlignMiddles16.png", "AlignMiddles24.png", "AlignMiddles32.png")]
	public static readonly string AlignMiddlesImage;

	[ImageResource("HideSelection16.png", "HideSelection24.png", "HideSelection32.png")]
	public static readonly string HideImage;

	[ImageResource("ShowSelection16.png", "ShowSelection24.png", "ShowSelection32.png")]
	public static readonly string ShowImage;

	[ImageResource("ShowSelection16.png", "ShowSelection24.png", "ShowSelection32.png")]
	public static readonly string ShowLastImage;

	[ImageResource("ShowAll16.png", "ShowAll24.png", "ShowAll32.png")]
	public static readonly string ShowAllImage;

	[ImageResource("Isolate16.png", "Isolate24.png", "Isolate32.png")]
	public static readonly string IsolateImage;

	[ImageResource("search.ico")]
	public static readonly string SearchImage;

	[ImageResource("filter.ico")]
	public static readonly string FilterImage;

	[ImageResource("ui_locked16x16.png", "ui_locked24x24.png", "ui_locked32x32.png")]
	public static readonly string LockUIImage;

	[ImageResource("ui_unlocked16x16.png", "ui_unlocked24x24.png", "ui_unlocked32x32.png")]
	public static readonly string UnlockUIImage;

	[ImageResource("Alphabetical.png")]
	public static readonly string AlphabeticalImage;

	[ImageResource("ByCategory.png")]
	public static readonly string ByCategoryImage;

	[ImageResource("nav_left.png")]
	public static readonly string NavLeftImage;

	[ImageResource("nav_right.png")]
	public static readonly string NavRightImage;

	[ImageResource("SCEA.png")]
	public static readonly string SceaImage;

	[ImageResource("AtfLogo.png")]
	public static readonly string AtfLogoImage;

	[ImageResource("AtfIcon.ico")]
	public static readonly string AtfIconImage;

	[ImageResource("Unsorted.png")]
	public static readonly string UnsortedImage;

	[ImageResource("factory.png")]
	public static readonly string FactoryImage;

	[ImageResource("elemdot.png")]
	public static readonly string ElementDotImage;

	[ImageResource("colpkg.png")]
	public static readonly string CollectionPackageImage;

	[ImageResource("package.png")]
	public static readonly string PackageImage;

	[ImageResource("package_error.png")]
	public static readonly string PackageErrorImage;

	[ImageResource("layer.png")]
	public static readonly string LayerImage;

	[ImageResource("add.png")]
	public static readonly string AddImage;

	[ImageResource("delete.png")]
	public static readonly string RemoveImage;

	[ImageResource("arrow_up_blue.png")]
	public static readonly string ArrowUpImage;

	[ImageResource("arrow_down_blue.png")]
	public static readonly string ArrowDownImage;

	[ImageResource("sort_ascending.png")]
	public static readonly string SortAscendingImage;

	[ImageResource("sort_descending.png")]
	public static readonly string SortDescendingImage;

	[ImageResource("checked_items.png")]
	public static readonly string CheckedItemsImage;

	[ImageResource("component16.png", "component24.png", "component32.png")]
	public static readonly string ComponentImage;

	[ImageResource("components.png")]
	public static readonly string ComponentsImage;

	[ImageResource("Reset16.png")]
	public static readonly string ResetImage;

	[ImageResource("greenPlus13.png")]
	public static readonly string GreenPlusIndicatorImage;

	[ImageResource("lock13.png")]
	public static readonly string LockIndicatorImage;

	[ImageResource("redCheck13.png")]
	public static readonly string RedCheckIndicatorImage;

	[ImageResource("unknown13.png")]
	public static readonly string UnknownIndicatorImage;

	[CursorResource("4WAY05.CUR")]
	public static readonly string FourWayCursor;

	[CursorResource("HO_SPLIT.CUR")]
	public static readonly string HorizSizeCursor;

	[CursorResource("VE_SPLIT.CUR")]
	public static readonly string VerticalSizeCursor;

	[CursorResource("SelectRow.cur")]
	public static readonly string RowSelectorCursor;

	[ImageResource("document_add.png")]
	public static readonly string DocumentAddImage;

	[ImageResource("document_check.png")]
	public static readonly string DocumentCheckOutImage;

	[ImageResource("document_into.png")]
	public static readonly string DocumentGetLatestImage;

	[ImageResource("document_lock.png")]
	public static readonly string DocumentLockImage;

	[ImageResource("document_refresh.png")]
	public static readonly string DocumentRefreshImage;

	[ImageResource("document_revert.png")]
	public static readonly string DocumentRevertImage;

	[ImageResource("document_warning.png")]
	public static readonly string DocumentWarningImage;

	[ImageResource("document_unknown.png")]
	public static readonly string DocumentUnknownImage;

	[ImageResource("sourceControl_enable.png")]
	public static readonly string SourceControlEnableImage;

	[ImageResource("sourceControl_disable.png")]
	public static readonly string SourceControlDisableImage;

	[ImageResource("sourceControl_add.png")]
	public static readonly string SourceControlAddImage;

	[ImageResource("sourceControl_checkin.png")]
	public static readonly string SourceControlCheckInImage;

	[ImageResource("sourceControl_checkout.png")]
	public static readonly string SourceControlCheckOutImage;

	[ImageResource("sourceControl_reconcile.png")]
	public static readonly string SourceControlReconcileImage;

	[ImageResource("sourceControl_connect.png")]
	public static readonly string SourceControlConnectionImage;

	[ImageResource("sourceControl_getLatest.png")]
	public static readonly string SourceControlGetLatestImage;

	[ImageResource("sourceControl_refresh.png")]
	public static readonly string SourceControlRefreshImage;

	[ImageResource("sourceControl_revert.png")]
	public static readonly string SourceControlRevertImage;

	[ImageResource("resource.png")]
	public static readonly string ResourceImage;

	[ImageResource("resourceFolder.png")]
	public static readonly string ResourceFolderImage;

	[ImageResource("reference.png")]
	public static readonly string ReferenceImage;

	[ImageResource("referenceNull.png")]
	public static readonly string ReferenceNullImage;

	[ImageResource("referenceOverride.png")]
	public static readonly string ReferenceOverrideImage;

	static Resources()
	{
		Type[] source = (from a in AppDomain.CurrentDomain.GetAssemblies()
			where !a.IsDynamic
			from t in a.GetExportedTypes()
			where t.Name == "ResourceUtil"
			select t).ToArray();
		IEnumerable<bool> source2 = from t in source
			from p in t.GetProperties()
			where p.Name == "RegistrationStarted" && p.PropertyType == typeof(bool) && p.GetGetMethod().IsPublic && p.GetGetMethod().IsStatic
			select (bool)p.GetGetMethod().Invoke(null, new object[0]);
		if (!source2.Any((bool p) => p))
		{
			MethodInfo[] array = (from t in source
				from m in t.GetMethods()
				where m.Name == "Register" && m.IsStatic && m.IsPublic && m.ReturnType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Type)
				select m).ToArray();
			if (array.Length > 1)
			{
				throw new InvalidOperationException("More than one library has implemented a ResourceUtil.Register(Type type).\nThis is allowed, but one or the other method must be called explicitly,\nbefore the app calls this static constructor.");
			}
			IEnumerable<PropertyInfo> source3 = from t in source
				from p in t.GetProperties()
				where p.Name == "RegistrationStarted" && p.PropertyType == typeof(bool) && p.GetSetMethod().IsPublic && p.GetSetMethod().IsStatic
				select p;
			source3.First().GetSetMethod().Invoke(null, new object[1] { true });
			Queue queue = new Queue(1);
			queue.Enqueue(typeof(Resources));
			array.First().Invoke(null, queue.ToArray());
		}
	}
}
